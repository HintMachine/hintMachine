namespace HintMachine.Games
{
    public class GeometryWarsConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 50000
        };

        public GeometryWarsConnector()
        {
            quests.Add(_scoreQuest);
        }

        public override string GetDisplayName()
        {
            return "Geometry Wars: Retro Evolved";
        }
        
        public override string GetDescription()
        {
            return "Kill geometric ennemies in this box shaped area twin stick shooter.\n\n" +
                   "Tested on up-to-date Steam version.";
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("GeometryWars");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long scoreAddress = _ram.ResolvePointerPath32(_ram.baseAddress + 0x170084, new int[] { 0x40 });
            _scoreQuest.UpdateValue(_ram.ReadUint32(scoreAddress));

            return true;
        }
    }
}