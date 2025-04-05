using HintMachine.Models.GenericConnectors;
using System;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    internal class SuperMarioBrosConnector : INESConnector
    {
        private bool cheated = false;
        private bool damageless = true;
        private int currentWorld = 0;
        private int currentLevel = 0;

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 100000,
            MaxIncrease = 8000,
            Description = "Collect 100,000 points"
        };

        private readonly HintQuestCumulative _worldQuest = new HintQuestCumulative
        {
            Name = "Worlds",
            GoalValue = 1,
            MaxIncrease = 1,
            Description = "Complete Bowser's Castle for a world"
        };

        private readonly HintQuestCumulative _streakQuest = new HintQuestCumulative
        {
            Name = "Streak",
            GoalValue = 5,
            MaxIncrease = 1,
            Description = "Complete 5 levels in a row without dying"
        };

        private readonly HintQuestCounter _damagelessQuest = new HintQuestCounter
        {
            Name = "Damageless",
            GoalValue = 1,
            MaxIncrease = 1,
            Description = "Complete a level without dying or taking damage"
        };

        public SuperMarioBrosConnector()
        {
            Name = "Super Mario Bros.";
            Description = "Bowser has taken over the Mushroom Kingdom, kidnapped their princess, and turned the residents into objects like blocks! Only Mario can save the day from the King of the Koopas and restore the people of the Mushroom Kingdom.\n\n"
                        + "Traverse through 8 worlds including lovely plains, underground caverns, treetops, underwater, giants mushrooms, and of course, Bowser's many castles. Can you manage to defeat his army of minions and save the princess?";
            SupportedVersions.Add("(World)");
            SupportedVersions.Add("(Japan, USA)");
            SupportedVersions.Add("(E)");
            CoverFilename = "super_mario_bros.png";
            Author = "RadzPrower";

            Quests.Add(_scoreQuest);
            Quests.Add(_worldQuest);
            Quests.Add(_streakQuest);
            Quests.Add(_damagelessQuest);

            ValidROMs.Add("F61548FDF1670CFFEFCC4F0B7BDCDD9EABA0C226E3B74F8666071496988248DE");
            ValidROMs.Add("EC299B990E8BFEE8BA46E3F61D63B2E1AE5B8A2E431DE84E2E4BBD692DC53586");
        }

        protected override bool Poll()
        {
            CheckState();
            CheckProgression();
            CheckScore();

            return true;
        }

        private void CheckState()
        {
            var state = _ram.ReadUint8(RamBaseAddress + 0xe);
            var deathMusic = _ram.ReadUint8(RamBaseAddress + 0x712);

            // State 11 is for dying from hitting an enemy or obstacle
            // State 6 is for dying in a pit
            if (deathMusic == 1)
            {
                _streakQuest.CurrentValue = 0;
                damageless = false;
                return;
            }

            // Taking damage
            if (state == 10) damageless = false;
        }

        private void CheckProgression()
        {
            var level = _ram.ReadUint8(RamBaseAddress + 0x760);
            var world = _ram.ReadUint8(RamBaseAddress + 0x75f);
            var autowalk = _ram.ReadUint8(RamBaseAddress + 0xe) == 7;

            // If the world is the next world and not a warp
            // currentLevel is checked to account for warping to the next world
            if ((world == (currentWorld + 1)) && currentLevel != 2)
            {
                _worldQuest.CurrentValue++;
                _streakQuest.CurrentValue++;

                if (damageless) _damagelessQuest.CurrentValue++;
                else damageless = true;

                // Update what world and level we are currently in
                currentWorld = world;
                currentLevel = level;

                return;
            }

            // Check if level transition was for autowalk segment
            if (autowalk) currentLevel++;

            // Check level for damageless completion
            if (level == (currentLevel + 1))
            {
                _streakQuest.CurrentValue++;

                if (damageless) _damagelessQuest.CurrentValue++;
                else damageless = true;
            }

            // Update what world and level we are currently in
            currentWorld = world;
            if (!autowalk) currentLevel = level;
        }

        private void CheckScore()
        {
            // Score is stored as individual digits,
            // so we create an array of the individual bytes
            byte[] scoreArray = new byte[7];
            for (byte i = 0; i < 6; i++)
            {
                scoreArray[i] = _ram.ReadUint8(RamBaseAddress + 0x7dd + i);
            }
            scoreArray[6] = 0;

            // Then we concatenate them into a single string which is then converted to a long
            var score = Convert.ToInt64(string.Concat(scoreArray));

            // Update the score for the quest if they haven't cheated
            if (cheated)
            {
                if (score == 0) cheated = false;
            }
            else
            {
                // Check if they cheated their score up beyond the max acceptable increase
                if ((_scoreQuest.CurrentValue + _scoreQuest.MaxIncrease) < (score % _scoreQuest.GoalValue))
                {
                    cheated = true;
                }

                _scoreQuest.UpdateValue(score);
            }
        }
    }
}
