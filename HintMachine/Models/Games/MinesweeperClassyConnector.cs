using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class MinesweeperClassyConnector : IGameConnector
    {
        private readonly HintQuestCumulative _beginnerWinsQuest = new HintQuestCumulative
        {
            Name = "Beginner Wins",
            GoalValue = 8,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _intermediateWinsQuest = new HintQuestCumulative
        {
            Name = "Intermediate Wins",
            GoalValue = 2,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _expertWinsQuest = new HintQuestCumulative
        {
            Name = "Expert Wins",
            GoalValue = 1,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _expertLargeWinsQuest = new HintQuestCumulative
        {
            Name = "Expert (Large) Wins",
            GoalValue = 1,
            MaxIncrease = 1,
            AwardedHints = 2,
        };

        private ProcessRamWatcher _ram = null;

        // ----------------------------------------------------

        public MinesweeperClassyConnector()
        {
            Name = "Minesweeper Classy";
            Description = "A modern and stylish take on a classic logic puzzle game. Play the classic minesweeper levels, beat unique level layouts and challenges, or create and share your own level designs.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "minesweeper_classy.png";
            Author = "Dinopony";

            Quests.Add(_beginnerWinsQuest);
            Quests.Add(_intermediateWinsQuest);
            Quests.Add(_expertWinsQuest);
            Quests.Add(_expertLargeWinsQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(new BinaryTarget
            {
                DisplayName = "Steam",
                ProcessName = "Minesweeper Classy",
                ModuleName = "mono-2.0-bdwgc.dll",
                Hash = "D6CD45D35A0A01F977C64C6A9843F9CA51567102EA5E097F1E2E86BE1D000AF4"
            });

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long statsStructureAddr = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x499430, new int[] { 0x158, 0x2A8, 0x5F0, 0x198 });
            if (statsStructureAddr != 0)
            {
                uint beginnerWins = _ram.ReadUint32(statsStructureAddr + 4);
                _beginnerWinsQuest.UpdateValue(beginnerWins);

                uint intermediateWins = _ram.ReadUint32(statsStructureAddr + 12);
                _intermediateWinsQuest.UpdateValue(intermediateWins);

                uint expertWins = _ram.ReadUint32(statsStructureAddr + 20);
                _expertWinsQuest.UpdateValue(expertWins);

                uint expertLargeWins = _ram.ReadUint32(statsStructureAddr + 28);
                _expertLargeWinsQuest.UpdateValue(expertLargeWins);
            }

            return true;
        }
    }
}