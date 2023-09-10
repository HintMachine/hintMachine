using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HintMachine
{
    public class ArchipelagoHintSession
    {
        private static readonly string[] TAGS = { "AP", "TextOnly" };
        private static readonly Version VERSION = new Version(0, 4, 1);

        private ArchipelagoSession _session = null;
        public bool isConnected = false;
        public string errorMessage = "";
        private List<long> _alreadyHintedLocations = new List<long>();

        public ArchipelagoHintSession(string host, string slot)
        {
            _session = ArchipelagoSessionFactory.CreateSession(host);

            Console.WriteLine("Start Connect & Login");
            LoginResult ret;
            try
            {
                ret = _session.TryConnectAndLogin("", slot, ItemsHandlingFlags.AllItems, VERSION, TAGS, null, null, true);
            }
            catch (Exception ex)
            {
                ret = new LoginFailure(ex.GetBaseException().Message);
            }

            isConnected = ret.Successful;
            if (!isConnected)
            {
                LoginFailure loginFailure = (LoginFailure)ret;
                errorMessage = "Failed to Connect ";
                foreach (string str in loginFailure.Errors)
                {
                    errorMessage += "\n    " + str;
                }
                foreach (ConnectionRefusedError connectionRefusedError in loginFailure.ErrorCodes)
                {
                    errorMessage += string.Format("\n    {0}", connectionRefusedError);
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

        public void getOneRandomHint()
        {
            List<long> missingLocations = _session.Locations.AllMissingLocations.ToList();
            foreach(long locationId in _alreadyHintedLocations)
                missingLocations.Remove(locationId);
            
            Random rnd = new Random();
            int index = rnd.Next(missingLocations.Count);
            long hintedLocationId = _session.Locations.AllMissingLocations[index];

            _alreadyHintedLocations.Add(hintedLocationId);
            Task<LocationInfoPacket> t = _session.Locations.ScoutLocationsAsync(true, hintedLocationId);
            LocationInfoPacket response = t.Result;
           
            string itemName = _session.Items.GetItemName(response.Locations[0].Item);
            string playerName = _session.Players.GetPlayerName(response.Locations[0].Player);
            string locationName = _session.Locations.GetLocationNameFromId(response.Locations[0].Location);
            Console.WriteLine(playerName + "'s " + itemName + " can be found at '" + locationName + "'");
        }
    }
}
