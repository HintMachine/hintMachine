using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class NexMachinaConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;

        private readonly HintQuestCumulative _humansQuest = new HintQuestCumulative
        {
            Name = "Humans saved",
            GoalValue = 50,
            MaxIncrease = 5,
            Description = "Secret humans also count, go find them !"
        };

        public NexMachinaConnector()
        {
            Name = "Nex Machina";
            Description = "Nex Machina is an explosive arcade experience created with competition in mind.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "nexmachina.png";
            Author = "CalDrac";

            Quests.Add(_humansQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("nex_machina");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long savesAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x00F5AEC8, new int[] { 0x20, 0xC8, 0x30, 0x20 });
            if (savesAddress != 0)
            {
                _humansQuest.UpdateValue(_ram.ReadUint32(savesAddress));
            }
            return true;
        }
    }
}