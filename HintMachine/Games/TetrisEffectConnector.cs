using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class TetrisEffectConnector : IGameConnector
    {
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
            CoverFilename = "tetris_effect_connected.png";
            Author = "Dinopony";

            Quests.Add(_linesQuest);
            Quests.Add(_tetrisesQuest);
            Quests.Add(_backToBackQuest);
            Quests.Add(_tspinsQuest);
            // Quests.Add(_perfectClearsQuest);
            // Quests.Add(_combosQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher(new BinaryTarget
            {
                DisplayName = "Steam",
                ProcessName = "TetrisEffect-Win64-Shipping",
                Hash = "1981E9829739E3C2E7549E9DEC3384A3E649632A514D9D5F0711A37CC945279D"
            });

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            int[] OFFSETS = new int[] { 0x8, 0x8, 0x220, 0x200, 0x64 };
            long linesAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x4ED9990, OFFSETS);
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
