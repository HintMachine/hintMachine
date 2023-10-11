namespace HintMachine.Games
{
    public class LuckBeALandlordConnector : IGameConnector
    {
        private readonly HintQuestCumulative _rentQuest = new HintQuestCumulative
        {
            Name = "Rent Payments Made",
            GoalValue = 2,
        };

        private ProcessRamWatcher _ram = null;

        public LuckBeALandlordConnector()
        {
            Name = "Luck be a Landlord";
            Description = "Roguelike deckbuilder about using a slot machine to earn rent money and defeat capitalism";
            SupportedVersions = "Steam";
            Author = "Serpent.AI";

            Quests.Add(_rentQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("Luck be a Landlord");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long rentAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x21F0580, new int[] { 0x8, 0x28, 0x70, 0x368, 0x10, 0xF8, 0x58, 0x20, 0x6F0 });
            _rentQuest.UpdateValue(_ram.ReadUint32(rentAddress + 0x8));

            return true;
        }
    }
}