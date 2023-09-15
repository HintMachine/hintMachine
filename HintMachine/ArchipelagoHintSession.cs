using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
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
        public bool isConnected = false;
        public string errorMessage = "";
        private List<long> _alreadyHintedLocations = new List<long>();

        public ArchipelagoHintSession(string host, string slot, string password)
        {
            this.host = host;
            this.slot = slot;
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

            // List all locations that were hinted at the time of connection
            // TODO: What about locations that are hinted outside of this client during exec?
            Hint[] hints = _session.DataStorage.GetHints();
            foreach(Hint hint in hints)
            {
                if(hint.FindingPlayer == _session.ConnectionInfo.Slot)
                    _alreadyHintedLocations.Add(hint.LocationId);
            }
        }
        public void GetOneRandomHint()
        {
            List<long> missingLocations = _session.Locations.AllMissingLocations.ToList();
            foreach (long locationId in _alreadyHintedLocations)
                missingLocations.Remove(locationId);

            if (missingLocations.Count == 0)
                return;

            Random rnd = new Random();
            int index = rnd.Next(missingLocations.Count);
            long hintedLocationId = _session.Locations.AllMissingLocations[index];

            _alreadyHintedLocations.Add(hintedLocationId);
            _session.Socket.SendPacket(new LocationScoutsPacket {
                Locations = new long[] { hintedLocationId },
                CreateAsHint = true
            });
        }

        public void SendMessage(string message)
        {
            _session.Socket.SendPacket(new SayPacket { Text = message });
        }

        public void SetupOnMessageReceivedEvent(MessageReceivedHandler handler)
        {
            _session.MessageLog.OnMessageReceived += handler;
        }
    }
}
