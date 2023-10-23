using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    public class TetrisEffectConnector : IGameConnector
    {
        private BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "TetrisEffect-Win64-Shipping",
            Hash = "1981E9829739E3C2E7549E9DEC3384A3E649632A514D9D5F0711A37CC945279D"
        };

        private BinaryTarget GAME_VERSION_EPIC = new BinaryTarget
        {
            DisplayName = "Epic",
            ProcessName = "TetrisEffect-Win64-Shipping",
            Hash = "45CF1A171161725BC6DFB3C3E3735580202BF5B5BE3A34769B378C503F94F83E"
        };


        private readonly HintQuestCumulative _linesQuest = new HintQuestCumulative
        {
            Name = "Cleared Lines",
            GoalValue = 200,
        };
        private readonly HintQuestCumulative _tetrisesQuest = new HintQuestCumulative
        {
            Name = "Tetris™",
            GoalValue = 25,
        };
        private readonly HintQuestCumulative _backToBackQuest = new HintQuestCumulative
        {
            Name = "Back-to-Back",
            GoalValue = 30,
        };
        private readonly HintQuestCumulative _tspinsQuest = new HintQuestCumulative
        {
            Name = "T-Spins",
            GoalValue = 40,
        };
        /*
        private readonly HintQuestCumulative _combosQuest = new HintQuestCumulative
        {
            Name = "Combos",
            GoalValue = 60,
        };
        private readonly HintQuestCumulative _perfectClearsQuest = new HintQuestCumulative
        {
            Name = "All Clears",
            GoalValue = 5,
        };
        */

        private ProcessRamWatcher _ram = null;

        // ------------------------------------------------------------------------

        public TetrisEffectConnector()
        {
            Name = "Tetris Effect: Connected";
            Description = "Named after a real-world phenomenon where players' brains are so engrossed that images of the iconic falling " +
                          "Tetrimino blocks linger in their vision, thoughts, and even dreams, Tetris Effect amplifies this magical feeling " +
                          "of total immersion by surrounding you with fantastic, fully three-dimensional worlds that react and evolve based " +
                          "on how you play. Music, backgrounds, sounds, special effects - everything, down to the Tetris pieces themselves, " +
                          "pulse, dance, shimmer, and explode in perfect sync with how you're playing.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            SupportedVersions.Add("Epic");
            CoverFilename = "tetris_effect_connected.png";
            Author = "Dinopony";

            Quests.Add(_linesQuest);
            Quests.Add(_tetrisesQuest);
            Quests.Add(_backToBackQuest);
            Quests.Add(_tspinsQuest);
            // Quests.Add(_perfectClearsQuest);
            // Quests.Add(_combosQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher();
            _ram.SupportedTargets.Add(GAME_VERSION_STEAM);
            _ram.SupportedTargets.Add(GAME_VERSION_EPIC);

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long baseOffset = (_ram.CurrentTarget == GAME_VERSION_STEAM) ? 0x4ED9990 : 0x4ECE880;
            int[] OFFSETS = new int[] { 0x8, 0x8, 0x220, 0x200, 0x64 };

            long linesAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + baseOffset, OFFSETS);
            if (linesAddress != 0)
            {
                _linesQuest.UpdateValue(_ram.ReadUint32(linesAddress));
                _tetrisesQuest.UpdateValue(_ram.ReadUint32(linesAddress + 0x40));
                _backToBackQuest.UpdateValue(_ram.ReadUint32(linesAddress + 0x50));
                _tspinsQuest.UpdateValue(_ram.ReadUint32(linesAddress + 0x70));

                // _perfectClearsQuest.UpdateValue(_ram.ReadUint32(linesAddress + 0xC0));
                // _combosQuest.UpdateValue(_ram.ReadUint32(combosAddress));
            }

            return true;
        }
    }
}
