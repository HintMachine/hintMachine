using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class TMNTShreddersRevengeConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "TMNT",
            Hash = "84DF2BBE0C0E45A83A1991C46635E11F32E7AADD4086E1F810EC1B61CB7B4952"
        };

        private readonly HintQuestCumulative _enemyQuest = new HintQuestCumulative
        {
            Name = "Enemies Defeated",
            GoalValue = 100,
            Description = "Story Mode, Arcade Mode",
            MaxIncrease = 10,
        };

        private readonly HintQuestCumulative _comboQuest = new HintQuestCumulative
        {
            Name = "Combo Points",
            GoalValue = 500,
            Description = "Story Mode, Arcade Mode",
            MaxIncrease = 10,
        };

        private readonly HintQuestCumulative _objectQuest = new HintQuestCumulative
        {
            Name = "Objects Destroyed",
            GoalValue = 25,
            Description = "Story Mode, Arcade Mode",
            MaxIncrease = 3,
        };

        private ProcessRamWatcher _ram = null;

        public TMNTShreddersRevengeConnector()
        {
            Name = "TMNT: Shredder's Revenge";
            Description = "Teenage Mutant Ninja Turtles: Shredder’s Revenge features groundbreaking gameplay rooted in timeless classic brawling mechanics, brought to you by the beat ’em up experts at Dotemu (Streets of Rage 4) and Tribute Games. Bash your way through gorgeous pixel art environments and slay tons of hellacious enemies with your favorite Turtle, each with his own skills and moves - making each run unique! Choose a fighter, use radical combos to defeat your opponents and experience intense combats loaded with breathtaking action and outrageous ninja abilities. Stay sharp as you face off against Shredder and his faithful Foot Clan alone, or grab your best buds and play with up to 6 players simultaneously!";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "tmnt_shredders_revenge.png";
            Author = "Serpent.AI";

            Quests.Add(_enemyQuest);
            Quests.Add(_comboQuest);
            Quests.Add(_objectQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher();
            _ram.SupportedTargets.Add(GAME_VERSION_STEAM);

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long gamePlayerInfoStructAddress = _ram.ResolvePointerPath64(_ram.Threadstack0 - 0xF70, new int[] { 0x30, 0x150, 0x80, 0x100, 0x28, 0x58 });

            if (gamePlayerInfoStructAddress != 0)
            {
                try
                {
                    long enemyValue = _ram.ReadUint32(gamePlayerInfoStructAddress + 0x64);
                    long comboValue = _ram.ReadUint32(gamePlayerInfoStructAddress + 0x34);
                    long objectValue = _ram.ReadUint32(gamePlayerInfoStructAddress + 0x84);

                    _enemyQuest.UpdateValue(enemyValue);
                    _comboQuest.UpdateValue(comboValue);
                    _objectQuest.UpdateValue(objectValue);
                }
                catch { }
            }

            return true;
        }
    }
}
