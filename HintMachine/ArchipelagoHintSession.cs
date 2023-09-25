using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HintMachine.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using static Archipelago.MultiClient.Net.Helpers.MessageLogHelper;

namespace HintMachine
{
    public class ArchipelagoHintSession
    {
        private static readonly string[] TAGS = { "AP", "TextOnly" };
        private static readonly Version VERSION = new Version(0, 4, 1);

        private ArchipelagoSession _session = null;
        public string host = "";
        public string slot = "";
        public string password = "";
        public bool isConnected = false;
        public string errorMessage = "";

        public List<HintDetails> KnownHints { get; set; } = new List<HintDetails>();

        public HintsView HintsView { get; set; } = null;

        public ArchipelagoHintSession(string host, string slot, string password)
        {
            this.host = host;
            this.slot = slot;
            this.password = password;
            _session = ArchipelagoSessionFactory.CreateSession(host);

            Console.WriteLine("Start Connect & Login");
            LoginResult ret;
            try
            {
                ret = _session.TryConnectAndLogin("", slot, ItemsHandlingFlags.AllItems, VERSION, TAGS, null, password, true);
            }
            catch (Exception ex)
            {
                ret = new LoginFailure(ex.GetBaseException().Message);
            }

            isConnected = ret.Successful;
            if (!isConnected)
            {
                LoginFailure loginFailure = (LoginFailure)ret;
                foreach (string str in loginFailure.Errors)
                {
                    errorMessage += "\n" + str;
                }
                foreach (ConnectionRefusedError connectionRefusedError in loginFailure.ErrorCodes)
                {
                    errorMessage += string.Format("\n{0}", connectionRefusedError);
                }
                return;
            }

            // Add a tracking event to detect further hints...
            _session.DataStorage.TrackHints(OnHintObtained, false);
            // ...and call that event a first time with all already obtained hints
            OnHintObtained(_session.DataStorage.GetHints());
        }

        public List<string> GetMissingLocationNames()
        {
            List<long> alreadyHintedLocations = GetAlreadyHintedLocations();

            List<string> returned = new List<string>();
            foreach (long id in _session.Locations.AllMissingLocations)
                if(!alreadyHintedLocations.Contains(id))
                    returned.Add(_session.Locations.GetLocationNameFromId(id));
            return returned;
        }

        public List<string> GetItemNames()
        {
            List<string> returned = new List<string>();
            int slotID = _session.ConnectionInfo.Slot;
            var game = _session.Players.AllPlayers.ElementAt(slotID).Game;
            var coll = _session.DataStorage.GetItemNameGroups(game);
            foreach (var itemName in coll["Everything"])
                returned.Add(itemName);

            return returned;
        }

        public void GetOneRandomHint(string gameName)
        {
            List<long> missingLocations = _session.Locations.AllMissingLocations.ToList();
            foreach (long locationId in GetAlreadyHintedLocations())
                missingLocations.Remove(locationId);

            if (missingLocations.Count == 0)
                return;

            Random rnd = new Random();
            int index = rnd.Next(missingLocations.Count);
            long hintedLocationId = _session.Locations.AllMissingLocations[index];
            
            SendMessage("I just found a hint using the HintMachine playing " + gameName + "!");
            _session.Socket.SendPacket(new LocationScoutsPacket {
                Locations = new long[] { hintedLocationId },
                CreateAsHint = true
            });
        }

        public void OnHintObtained(Hint[] hints)
        {
            // Add the hints to the list of already known locations so that we won't 
            // try to give a random hint for those
            KnownHints.Clear();
            foreach (Hint hint in hints)
            {
                string locationName = _session.Locations.GetLocationNameFromId(hint.LocationId);
                if (hint.Entrance != "")
                    locationName += " (" + hint.Entrance + ")"; 

                KnownHints.Add(new HintDetails
                {
                    ReceivingPlayer = hint.ReceivingPlayer,
                    FindingPlayer = hint.FindingPlayer,
                    ItemId = hint.ItemId,
                    LocationId = hint.LocationId,
                    ItemFlags = hint.ItemFlags,
                    Found = hint.Found,
                    Entrance = hint.Entrance,

                    ReceivingPlayerName = _session.Players.GetPlayerName(hint.ReceivingPlayer),
                    FindingPlayerName = _session.Players.GetPlayerName(hint.FindingPlayer),
                    ItemName = _session.Items.GetItemName(hint.ItemId),
                    LocationName = locationName,
                });
            }

            HintsView?.UpdateItems(KnownHints);
        }

        public List<long> GetAlreadyHintedLocations()
        {
            List<long> returned = new List<long>();
            foreach (HintDetails hint in KnownHints)
                if (hint.FindingPlayer == _session.ConnectionInfo.Slot)
                    returned.Add(hint.LocationId);

            return returned;
        }

        public int GetAvailableHintsWithHintPoints()
        {
            int points = _session.RoomState.HintPoints;
            int cost = (int)(_session.Locations.AllLocations.Count * 0.01m * _session.RoomState.HintCostPercentage);
            return points / cost;
        }

        public int GetCheckCountBeforeNextHint()
        {
            int points = _session.RoomState.HintPoints;
            int cost = (int)(_session.Locations.AllLocations.Count * 0.01m * _session.RoomState.HintCostPercentage);
            while (points >= cost)
                points -= cost;

            int pointsToNextHint = cost - points;
            int pointsPerCheck = _session.RoomState.LocationCheckPoints;

            return (int)Math.Ceiling((float)pointsToNextHint / (float)pointsPerCheck);
        }

        public void Disconnect()
        {
            _session.Socket.DisconnectAsync();
        }

        public void SendMessage(string message)
        {
            _session.Socket.SendPacket(new SayPacket { Text = message });
        }

        public void SetupOnMessageReceivedEvent(MessageReceivedHandler handler)
        {
            _session.MessageLog.OnMessageReceived += handler;
        }

        public List<string> GetPlayerNames()
        {
            List<string> names = new List<string>();
            foreach (PlayerInfo info in _session.Players.AllPlayers)
                names.Add(info.Name);
            return names;
        }
    }
}
