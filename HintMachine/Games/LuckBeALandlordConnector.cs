namespace HintMachine.Games
{
    public class LuckBeALandlordConnector : IGameConnector
    {
        private readonly HintQuestCumulative _coinQuest = new HintQuestCumulative
        {
            Name = "Coins Spent",
            GoalValue = 375,
            Direction = HintQuestCumulative.CumulativeDirection.DESCENDING
        };

        private ProcessRamWatcher _ram = null;

        public LuckBeALandlordConnector()
        {
            Name = "Luck be a Landlord";
            Description = "Roguelike deckbuilder about using a slot machine to earn rent money and defeat capitalism";
            SupportedVersions = "Steam";
            Author = "Serpent.AI";

            Quests.Add(_coinQuest);
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
            long coinAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x21EFC90, new int[] { 0x218, 0x108, 0x10, 0x58, 0x20, 0x660 });
            long coinValue = (long)_ram.ReadDouble(coinAddress + 0x8);

            if (coinValue > 1)
            {
                _coinQuest.UpdateValue(coinValue);
            }
            else
            {
                _coinQuest.ForceLastMemoryReading(coinValue);
            }

            return true;
        }
    }
}