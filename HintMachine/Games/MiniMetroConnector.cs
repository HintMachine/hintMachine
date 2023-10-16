namespace HintMachine.Games
{
    public class MiniMetroConnector : IGameConnector
    {
        private readonly HintQuestCumulative _passengersQuest = new HintQuestCumulative
        {
            Name = "Passengers Delivered",
            GoalValue = 150,
            MaxIncrease = 10,
        };

        private ProcessRamWatcher _ram = null;

        public MiniMetroConnector()
        {
            Name = "Mini Metro";
            Description = "Mini Metro is a strategy simulation game about designing a subway map for a growing city.\n\nDraw lines between stations and start your trains running. As new stations open, redraw your lines to keep them efficient. Decide where to use your limited resources. How long can you keep the city moving?";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "mini_metro.png";
            Author = "Chandler";

            Quests.Add(_passengersQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("MiniMetro", "mono-2.0-bdwgc.dll");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            try {
                long address = _ram.ResolvePointerPath32(_ram.BaseAddress + 0x3A1574, new int[] { 0x690, 0x20, 0x8, 0x4C, 0x8, 0xC, 0x88 });
                _passengersQuest.UpdateValue(_ram.ReadUint32(address));
            }
            catch
            { }
            
            return true;
        }
    }
}
