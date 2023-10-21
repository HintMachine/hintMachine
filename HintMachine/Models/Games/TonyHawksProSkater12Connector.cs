using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    public class TonyHawksProSkater12Connector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "THPS12",
            Hash = "7EE922549418733002992C54C02BA6ED23DA90564C173B4AFF27AF561F534B25"
        };

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            Description = "Single Player (Skate Tours): THPS Parks, THPS2 Parks, Ranked Skate - Single Session",
            GoalValue = 250000,
        };

        private ProcessRamWatcher _ram = null;

        public TonyHawksProSkater12Connector()
        {
            Name = "Tony Hawk's Pro Skater 1 + 2";
            Description = "Play the fully-remastered Tony Hawk’s™ Pro Skater™ & Tony Hawk’s™ Pro Skater™ 2 games in one epic collection, rebuilt from the ground up in incredible HD.\r\n\r\nSkate as the legendary Tony Hawk and the original pro roster, plus new pros. Listen to songs from the era-defining soundtrack along with new music. Hit insane trick combos with the iconic handling of the Tony Hawk’s™ Pro Skater™ series.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "tony_hawks_pro_skater_1_2.png";
            Author = "Serpent.AI";

            Quests.Add(_scoreQuest);
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
            long scoreAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x3C40A20, new int[] { 0x0, 0x48, 0x0, 0x20, 0x478, 0x298, 0x2D8 });

            if (scoreAddress != 0)
            {
                try
                {
                    long scoreValue = _ram.ReadUint32(scoreAddress);

                    if (scoreValue <= 25000000)
                    {
                        _scoreQuest.UpdateValue(scoreValue);
                    }
                }
                catch { }
            }

            return true;
        }
    }
}