using System;

namespace HintMachine.Models
{
    public class HintDetails : Archipelago.MultiClient.Net.Models.Hint, IEquatable<HintDetails>
    {
        public string ReceivingPlayerName { get; set; }

        public string FindingPlayerName { get; set; }

        public string ItemName { get; set; }

        public string LocationName { get; set; }

        public bool Equals(HintDetails other)
        {
            if(other == null)
                return false;

            // HintDetails properties
            return ReceivingPlayerName == other.ReceivingPlayerName &&
                FindingPlayerName == other.FindingPlayerName &&
                ItemName == other.ItemName &&
                LocationName == other.LocationName &&
                ReceivingPlayer == other.ReceivingPlayer &&
                FindingPlayer == other.FindingPlayer &&
                ItemId == other.ItemId &&
                LocationId == other.LocationId &&
                ItemFlags == other.ItemFlags &&
                Found == other.Found &&
                Entrance == other.Entrance;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is HintDetails personObj)
                return Equals(personObj);
            else
                return false;
        }
    }
}
