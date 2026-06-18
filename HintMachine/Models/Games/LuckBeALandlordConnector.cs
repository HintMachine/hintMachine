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

        private static readonly int[] COIN_OFFSETS = new int[] { 0x218, 0x108, 0x10, 0x58, 0x20, 0x750 };
        private static readonly int[] SPIN_OFFSETS = new int[] { 0x218, 0x108, 0x38, 0x108, 0x0, 0x58, 0x20, 0x300 };

        private const int GODOT_VARIANT_TYPE_INT = 2;
        private const int GODOT_VARIANT_TYPE_REAL = 3;
        private const int GODOT_VARIANT_VALUE_OFFSET = 0x8;

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
            _bufferedSpinCount = null;
        }

        protected override bool Poll()
        {
            long coinStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x2048900, COIN_OFFSETS);
            long spinStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x2048900, SPIN_OFFSETS);

            if (coinStructAddress == 0 || spinStructAddress == 0)
            {
                _coinQuest.IgnoreNextValue();
                _bufferedSpinCount = null;
                return true;
            }

            try
            {
                long coinValue = ReadGodotNumericVariant(coinStructAddress);
                long spinValue = ReadGodotNumericVariant(spinStructAddress);

                // Check if spin count reset (new round/game state change)
                if (_bufferedSpinCount != null && spinValue < (long)_bufferedSpinCount)
                {
                    _coinQuest.IgnoreNextValue();
                }

                _coinQuest.UpdateValue(coinValue);
                _bufferedSpinCount = spinValue;
            }
            catch
            {
                _coinQuest.IgnoreNextValue();
                _bufferedSpinCount = null;
            }

            return true;
        }

        private long ReadGodotNumericVariant(long variantAddress)
        {
            uint variantType = _ram.ReadUint32(variantAddress);

            return variantType switch
            {
                GODOT_VARIANT_TYPE_INT => _ram.ReadInt64(variantAddress + GODOT_VARIANT_VALUE_OFFSET),
                GODOT_VARIANT_TYPE_REAL => (long)_ram.ReadDouble(variantAddress + GODOT_VARIANT_VALUE_OFFSET),
                _ => throw new ProcessRamWatcherException($"Unexpected Godot Variant type {variantType} at 0x{variantAddress:X}")
            };
        }
    }
}
