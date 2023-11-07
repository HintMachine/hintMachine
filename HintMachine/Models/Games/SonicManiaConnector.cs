using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class SonicManiaConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "SonicMania",
            Hash = "69DA0C7B1B9D54ACE4A84B500A1610E761E55F155969442D6D036C86E9DDA8A7"
        };

        private readonly HintQuestCumulative _ringQuest = new HintQuestCumulative
        {
            Name = "Rings Collected Over 20",
            Description = "Mania Mode, Encore Mode, Time Attack",
            GoalValue = 150,
        };

        private ProcessRamWatcher _ram = null;

        private long _previousCharacterValue = 0;

        public SonicManiaConnector()
        {
            Name = "Sonic Mania";
            Description = "Sonic Mania is an all-new adventure with Sonic, Tails, and Knuckles full of unique bosses, rolling 2D landscapes, and fun classic gameplay.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "sonic_mania.png";
            Author = "Serpent.AI";

            Quests.Add(_ringQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(GAME_VERSION_STEAM);
            _ram.Is64Bit = false;

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long characterValue = _ram.ReadUint32(_ram.BaseAddress + 0x469AD0);
            string characterName = "Sonic";

            long ringValue = _ram.ReadUint32(_ram.BaseAddress + 0x469AD4);
            if (ringValue <= 20) { ringValue = 20; }

            if (characterValue > 0 && characterValue <= 16 && _previousCharacterValue != 0)
            {
                switch (characterValue)
                {
                    case 2:
                        characterName = "Tails";
                        break;
                    case 4:
                        characterName = "Knuckles";
                        break;
                    case 8:
                        characterName = "Mighty";
                        break;
                    case 16:
                        characterName = "Ray";
                        break;
                }

                _ringQuest.Name = $"Rings Collected Over 20 ({characterName})";
                _ringQuest.UpdateValue(ringValue);
            }
            else
            {
                _ringQuest.Name = "Rings Collected Over 20";
                _ringQuest.IgnoreNextValue();
            }

            _previousCharacterValue = characterValue;

            return true;
        }
    }
}