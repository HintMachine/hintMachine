using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class LuckBeALandlordConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam + Itch",
            ProcessName = "Luck be a Landlord",
            Hash = "29A32075AB8D092BDDC107FE0A1B6BF21192F67D665525C2B1218E9C754608E0"
        };

        private readonly HintQuestCumulative _coinQuest = new HintQuestCumulative
        {
            Name = "Coins Spent",
            GoalValue = 375,
            Direction = HintQuestCumulative.CumulativeDirection.DESCENDING
        };

        private ProcessRamWatcher _ram = null;

        private long? _bufferedSpinCount = null;

        public LuckBeALandlordConnector()
        {
            Name = "Luck be a Landlord";
            Description = "Roguelike deckbuilder about using a slot machine to earn rent money and defeat capitalism. Your landlord is knocking on your door. You have one coin left to your name. You insert the coin into your slot machine...and...JACKPOT! Luck be a Landlord, tonight!";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "luck_be_a_landord.png";
            Author = "Serpent.AI";

            Quests.Add(_coinQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(GAME_VERSION_STEAM);
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long coinStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x2048900, new int[] { 0x218, 0x108, 0x10, 0x58, 0x20, 0x678 });
            long spinStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x2048900, new int[] { 0x218, 0x108, 0x38, 0x108, 0x0, 0x58, 0x20, 0x300 });

            if (coinStructAddress != 0 && spinStructAddress != 0)
            {
                try
                {
                    long coinValue = (long)_ram.ReadDouble(coinStructAddress + 0x8);
                    long spinValue = _ram.ReadUint32(spinStructAddress + 0x8);

                    if (_bufferedSpinCount == null) { _bufferedSpinCount = spinValue; }

                    if (spinValue < (long)_bufferedSpinCount) { _coinQuest.IgnoreNextValue(); }

                    _coinQuest.UpdateValue(coinValue);

                    _bufferedSpinCount = spinValue;
                }
                catch { }
            }

            return true;
        }
    }
}