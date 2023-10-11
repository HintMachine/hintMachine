using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HintMachine.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HintMachine
{
    public class ArchipelagoHintSession
    {
        private static readonly string[] TAGS = { "AP", "HintGenerator", "TextOnly" };
        private static readonly Version VERSION = new Version(0, 4, 1);

        public ArchipelagoSession Client { get; private set; }

        /// <summary>
        /// The hostname of the Archipelago server used for this session
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// The name of the slot (player) we are connected as for this session
        /// </summary>
        public string Slot { get; private set; }

        /// <summary>
        /// The password that was used to attempt connection. It needs to be kept for fast reconnect
        /// features where it will be reused to connect to another slot for the same host.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// A boolean set to true if connection to the Archipelago server was successful, false otherwise.
        /// If false, try reading the contents of the ErrorMessage property to get the cause.
        /// </summary>
        public bool IsConnected { get; set; } = false;

        /// <summary>
        /// The cause of the connection failure if there was one.
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// An exhaustive list of currently known hints with additionnal details
        /// </summary>
        public List<HintDetails> KnownHints { get; private set; } = new List<HintDetails>();

        public delegate void HintsUpdateHandler(List<HintDetails> hints);
        /// <summary>
        /// An event triggered when a new hint is obtained
        /// </summary>
        public event HintsUpdateHandler OnHintsUpdate;

        /// <summary>
        /// The amount of remaining random hints the client has yet to ask the server
        /// </summary>
        public int PendingRandomHints
        {
            get { return _pendingRandomHints; }
            set { 
                _pendingRandomHints = value;
                if (_pendingRandomHints > Globals.PendingHintsQueueMaxSize)
                    _pendingRandomHints = Globals.PendingHintsQueueMaxSize;
            }
        }
        private int _pendingRandomHints = 0;

        /// <summary>
        /// A thread responsible for requesting new random hints when PendingRandomHints > 0
        /// </summary>
        private readonly Thread _hintQueueThread = null;

        // ----------------------------------------------------------------------------------

        public ArchipelagoHintSession(string host, string slot, string password)
        {
            Host = host;
            Slot = slot;
            Password = password;
            Client = ArchipelagoSessionFactory.CreateSession(host);

            Console.WriteLine("Start Connect & Login");
            LoginResult ret;
            try
            {
                ret = Client.TryConnectAndLogin("", slot, ItemsHandlingFlags.IncludeOwnItems, VERSION, TAGS, null, password, true);

                _hintQueueThread = new Thread(HintQueueThreadLoop);
                _hintQueueThread.IsBackground = true;
                _hintQueueThread.Start();
            }
            catch (Exception ex)
            {
                ret = new LoginFailure(ex.GetBaseException().Message);
            }

            IsConnected = ret.Successful;
            if (!IsConnected)
            {
                LoginFailure loginFailure = (LoginFailure)ret;
                foreach (string str in loginFailure.Errors)
                {
                    ErrorMessage += "\n" + str;
                }
                foreach (ConnectionRefusedError connectionRefusedError in loginFailure.ErrorCodes)
                {
                    ErrorMessage += string.Format("\n{0}", connectionRefusedError);
                }
                return;
            }

            Client.MessageLog.OnMessageReceived += OnArchipelagoMessageReceived;

            // Add a tracking event to detect further hints...
            Client.DataStorage.TrackHints(OnHintReceivedHandler, false);
//            Client.Items.ItemReceived += OnItemReceivedHandler;

            // ...and call that event a first time with all already obtained hints
            Client.DataStorage[$"_read_hints_{Client.ConnectionInfo.Team}_{Client.ConnectionInfo.Slot}"].GetAsync<Hint[]>().ContinueWith(x => 
            {
                OnHintReceivedHandler(x.Result);
            });
        }

        public List<string> GetMissingLocationNames()
        {
            List<long> alreadyHintedLocations = GetAlreadyHintedLocations();

            List<string> returned = new List<string>();
            foreach (long id in Client.Locations.AllMissingLocations)
                if(!alreadyHintedLocations.Contains(id))
                    returned.Add(Client.Locations.GetLocationNameFromId(id));

            returned.Sort();
            return returned;
        }

        public List<string> GetItemNames()
        {
            List<string> returned = new List<string>();
            int slotID = Client.ConnectionInfo.Slot;
            var game = Client.Players.AllPlayers.ElementAt(slotID).Game;
            var coll = Client.DataStorage.GetItemNameGroups(game);
            foreach (var itemName in coll["Everything"])
                returned.Add(itemName);

            return returned;
        }

        public void HintQueueThreadLoop()
        {
            while (true)
            {
                if (PendingRandomHints > 0)
                {
                    PendingRandomHints -= 1;

                    List<long> missingLocations = Client.Locations.AllMissingLocations.ToList();
                    foreach (long locationId in GetAlreadyHintedLocations())
                        missingLocations.Remove(locationId);

                    if (missingLocations.Count > 0)
                    {
                        Random rnd = new Random();
                        int index = rnd.Next(missingLocations.Count);
                        long pendingHintLocationID = Client.Locations.AllMissingLocations[index];

                        Client.Socket.SendPacketAsync(new LocationScoutsPacket
                        {
                            Locations = new long[] { pendingHintLocationID },
                            CreateAsHint = true
                        });
                        Thread.Sleep(Globals.HintQueueInterval);
                    }
                }

                Thread.Sleep(Globals.TickInterval);
            }
        }
        
        /*
        private void OnItemReceivedHandler(ReceivedItemsHelper helper)
        {
            try
            {
                while (true)
                {
                    NetworkItem nextItem = helper.DequeueItem();
                    foreach (HintDetails hint in KnownHints)
                    {
                        if (hint.LocationId != nextItem.Location)
                            continue;
                        if (hint.FindingPlayer != nextItem.Player)
                            continue;

                        Console.WriteLine($"Removed hint with location #{hint.LocationId}");
                        hint.Found = true;
                        break;
                    }
                }
            }
            catch(InvalidOperationException) { }
        }
        */

        private void OnHintReceivedHandler(Hint[] hints)
        {
            // Add the hints to the list of already known locations so that we won't 
            // try to give a random hint for those
            KnownHints.Clear();
            foreach (Hint hint in hints)
            {
                string locationName = Client.Locations.GetLocationNameFromId(hint.LocationId);
                if (hint.Entrance != "")
                    locationName += $" ({hint.Entrance})";

                KnownHints.Add(new HintDetails
                {
                    ReceivingPlayer = hint.ReceivingPlayer,
                    FindingPlayer = hint.FindingPlayer,
                    ItemId = hint.ItemId,
                    LocationId = hint.LocationId,
                    ItemFlags = hint.ItemFlags,
                    Found = hint.Found,
                    Entrance = hint.Entrance,

                    ReceivingPlayerName = Client.Players.GetPlayerName(hint.ReceivingPlayer),
                    FindingPlayerName = Client.Players.GetPlayerName(hint.FindingPlayer),
                    ItemName = Client.Items.GetItemName(hint.ItemId),
                    LocationName = locationName,
                });
            }

            OnHintsUpdate?.Invoke(KnownHints);
        }

        private void OnArchipelagoMessageReceived(LogMessage message)
        {
            LogMessageType type = LogMessageType.RAW;
            List<MessagePart> parts = Enumerable.ToList(message.Parts);

            if (message is JoinLogMessage || message is LeaveLogMessage)
                type = LogMessageType.JOIN_LEAVE;
            else if (message is HintItemSendLogMessage)
            {
                type = LogMessageType.HINT;
                parts.RemoveAt(0); // Remove the [Hint] prefix
            }
            else if (message is ItemSendLogMessage itemMessage)
            {
                if (itemMessage.Sender.Name == Slot)
                    type = LogMessageType.ITEM_SENT;
                else if (itemMessage.Receiver.Name == Slot)
                    type = LogMessageType.ITEM_RECEIVED;
                else
                    return;
            }
            else if (message is ChatLogMessage || message is ServerChatLogMessage)
                type = LogMessageType.CHAT;
            else if (message is CommandResultLogMessage)
                type = LogMessageType.SERVER_RESPONSE;
            else if (message is GoalLogMessage)
                type = LogMessageType.GOAL;
            else if (message is TutorialLogMessage)
                return;

            string str = "";
            foreach (var part in parts)
                str += part.Text;

            Logger.Log(str, type);
        }

        public List<long> GetAlreadyHintedLocations()
        {
            List<long> returned = new List<long>();
            foreach (HintDetails hint in KnownHints)
                if (hint.FindingPlayer == Client.ConnectionInfo.Slot)
                    returned.Add(hint.LocationId);

            return returned;
        }

        public int GetAvailableHintsWithHintPoints()
        {
            int points = Client.RoomState.HintPoints;
            int cost = (int)(Client.Locations.AllLocations.Count * 0.01m * Client.RoomState.HintCostPercentage);
            if (cost == 0)
                return int.MaxValue;
            return points / cost;
        }

        public int GetCheckCountBeforeNextHint()
        {
            int points = Client.RoomState.HintPoints;
            int cost = (int)(Client.Locations.AllLocations.Count * 0.01m * Client.RoomState.HintCostPercentage);
            if (cost == 0)
                return 0;
            while (points >= cost)
                points -= cost;

            int pointsToNextHint = cost - points;
            int pointsPerCheck = Client.RoomState.LocationCheckPoints;
            if (pointsPerCheck == 0)
                return int.MaxValue;

            return (int)Math.Ceiling((float)pointsToNextHint / (float)pointsPerCheck);
        }

        public void Disconnect()
        {
            _hintQueueThread.Abort();
            Client.Socket.DisconnectAsync();
        }

        public void SendMessage(string message)
        {
            Client.Socket.SendPacketAsync(new SayPacket { Text = message });
        }

        public List<string> GetPlayerNames()
        {
            List<string> names = new List<string>();
            foreach (PlayerInfo info in Client.Players.AllPlayers)
                names.Add(info.Name);
            return names;
        }
    }
}
