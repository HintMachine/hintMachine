using System;

using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    public class TonyHawksProSkater12Connector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "THPS12",
            Hash = "1334052BB57D766AE1B4659F9C12ABC7AFDE5FC6DD80E0457FB1885B909233E9"
        };

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            Description = "Single Player (Skate Tours): THPS Parks, THPS2 Parks",
            GoalValue = 500000,
        };

        private readonly HintQuestCumulative _grindQuest = new HintQuestCumulative
        {
            Name = "Longest Grind Time (Run)",
            Description = "Single Player (Skate Tours): THPS Parks, THPS2 Parks",
            GoalValue = 15,
        };

        private readonly HintQuestCumulative _manualQuest = new HintQuestCumulative
        {
            Name = "Longest Manual Time (Run)",
            Description = "Single Player (Skate Tours): THPS Parks, THPS2 Parks",
            GoalValue = 30,
        };

        private readonly HintQuestCumulative _lipQuest = new HintQuestCumulative
        {
            Name = "Longest Lip Time (Run)",
            Description = "Single Player (Skate Tours): THPS Parks, THPS2 Parks",
            GoalValue = 20,
        };

        private readonly HintQuestCumulative _comboQuest = new HintQuestCumulative
        {
            Name = "Best Combo (Run)",
            Description = "Single Player (Skate Tours): THPS Parks, THPS2 Parks",
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
            Quests.Add(_grindQuest);
            Quests.Add(_manualQuest);
            Quests.Add(_lipQuest);
            Quests.Add(_comboQuest);
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
            long statsStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x3934090, new int[] { 0xD20, 0xB0, 0x20, 0xA8, 0x50, 0x0 });

            if (statsStructAddress != 0)
            {
                try
                {
                    long scoreValue = _ram.ReadUint32(statsStructAddress + 0x300);
                    float grindValue = _ram.ReadFloat(statsStructAddress + 0x390);
                    float manualValue = _ram.ReadFloat(statsStructAddress + 0x394);
                    float lipValue = _ram.ReadFloat(statsStructAddress + 0x398);
                    long comboValue = _ram.ReadUint32(statsStructAddress + 0x350);

                    _scoreQuest.UpdateValue(scoreValue);
                    _grindQuest.UpdateValue((long)Math.Round(grindValue));
                    _manualQuest.UpdateValue((long)Math.Round(manualValue));
                    _lipQuest.UpdateValue((long)Math.Round(lipValue));
                    _comboQuest.UpdateValue(comboValue);
                }
                catch { }
            }

            return true;
        }
    }
}