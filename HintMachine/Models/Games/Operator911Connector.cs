using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class Operator911Connector : IGameConnector
    {
        private readonly HintQuestCumulative _incidentsQuest = new HintQuestCumulative
        {
            Name = "Incidents Resolved",
            GoalValue = 15,
            MaxIncrease = 3,
        };

        private readonly HintQuestCumulative _cashQuest = new HintQuestCumulative
        {
            Name = "Cash Earned",
            GoalValue = 40000,
            MaxIncrease = 15000,
        };

        private ProcessRamWatcher _ram = null;

        public Operator911Connector()
        {
            Name = "911 Operator";
            Description = "In 911 Operator, you take on the role of an emergency dispatcher, who has to rapidly deal with the incoming reports. Your task is not just to pick up the calls, but also to react appropriately to the situation – sometimes giving first aid instructions is enough, at other times a police, fire department or paramedics’ intervention is a necessity. Keep in mind, that the person on the other side of the line might turn out to be a dying daughter’s father, an unpredictable terrorist, or just a prankster. Can you handle all of this?";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "911_operator.png";
            Author = "Chandler";

            Quests.Add(_incidentsQuest);
            Quests.Add(_cashQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("911");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long incidentsAddress = _ram.ResolvePointerPath32(_ram.BaseAddress + 0x1037A40, new int[] { 0x4, 0x8, 0x50, 0x68, 0x38, 0x164, 0x14 });
            if(incidentsAddress != 0)
                _incidentsQuest.UpdateValue(_ram.ReadUint32(incidentsAddress));

            long cashAddress = _ram.ResolvePointerPath32(_ram.BaseAddress + 0xFD3730, new int[] { 0x4, 0x4, 0x8, 0x18, 0x3C, 0x14, 0x18C });
            if(cashAddress != 0)
                _cashQuest.UpdateValue(_ram.ReadUint32(cashAddress));
            
            return true;
        }
    }
}
