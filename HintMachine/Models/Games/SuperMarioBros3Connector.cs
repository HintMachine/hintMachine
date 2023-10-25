using HintMachine.Models.GenericConnectors;
using System;

namespace HintMachine.Models.Games
{
    internal class SuperMarioBros3Connector : INESConnector
    {
        private bool inLevel = false;
        private bool notPipe = true;
        private int levelsCleared = 0;
        private int currentStreak = 0;
        private int currentWorld = 0;
        private int worldsCompleted = 0;

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 100000,
            MaxIncrease = 8000,
        };

        private readonly HintQuestCumulative _levelQuest = new HintQuestCumulative
        {
            Name = "Levels",
            GoalValue = 3,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 2,
        };

        private readonly HintQuestCumulative _worldQuest = new HintQuestCumulative
        {
            Name = "Worlds",
            GoalValue = 1,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _streakQuest = new HintQuestCumulative
        {
            Name = "Streak",
            GoalValue = 10,
            MaxIncrease = 1,
            CooldownBetweenIncrements = 2,
        };

        public SuperMarioBros3Connector()
        {
            Name = "Super Mario Bros. 3";
            Description = "Mario and friends put on a play to tell a story of Bowser attacking kingdoms around the world. Travel across 8 lands and save them from the Koopalings who have transformed the leaders of these lands into assorted creatures!";
            SupportedVersions.Add("(USA)");
            CoverFilename = "super_mario_bros_3.png";
            Author = "RadzPrower";

            Quests.Add(_scoreQuest);
            Quests.Add(_levelQuest);
            Quests.Add(_worldQuest);
            Quests.Add(_streakQuest);

            ValidROMs.Add("6EA0777CA520BA7AD7B0EA0F6452140D59AA0B1DFFE5045CB1E8020DDE10C267");
        }

        protected override bool Poll()
        {
            CheckScore();

            if (!inLevel) CheckTimer();

            if (inLevel) CheckExit();

            // Update world number

            int RAMWorld = _ram.ReadUint8(RamBaseAddress + 0x727);
            if (RAMWorld == (currentWorld + 1))
            {
                currentWorld = RAMWorld;
                worldsCompleted++;
            }
            else if (RAMWorld >= 8)
            {
                currentWorld = -1;
            }
            else if (RAMWorld > currentWorld)
            {
                currentWorld = RAMWorld;
            }

            _worldQuest.UpdateValue(worldsCompleted);

            return true;
        }

        private void CheckTimer()
        {
            string timerString = _ram.ReadUint8(RamBaseAddress + 0x5ee).ToString()
                         + _ram.ReadUint8(RamBaseAddress + 0x5ef).ToString()
                         + _ram.ReadUint8(RamBaseAddress + 0x5f0).ToString();

            int timer = Convert.ToInt16(timerString);

            if (timer > 0) inLevel= true;
        }

        private void CheckScore()
        {
            // Read 3 bytes where score is stored
            byte[] byteArray = _ram.ReadBytes(RamBaseAddress + 0x0715, 3);

            // Concatenate the bytes to create a single hex value
            string hexScore = byteArray[0].ToString("X2") + byteArray[1].ToString("X2") + byteArray[2].ToString("X2");

            // Convert that hex string into a decimal value and multiply by 10 to get the final score
            int score = Convert.ToInt32(hexScore, 16) * 10;

            // Update the score for the quest
            _scoreQuest.UpdateValue(score);
        }

        private void CheckExit()
        {
            if ((_ram.ReadUint8(RamBaseAddress + 0x545) == 3) || (_ram.ReadUint8(RamBaseAddress + 0x545) == 2))
                if (_ram.ReadUint8(RamBaseAddress + 0x70a) == 14) notPipe = false;

            if (_ram.ReadUint8(RamBaseAddress + 0x14) == 1)
            {
                if (_ram.ReadUint8(RamBaseAddress + 0x713) == 0)
                {
                    if (notPipe)
                    {
                        levelsCleared++;
                        currentStreak++;
                    }
                }
                else if (notPipe)
                {
                    currentStreak = 0;
                    _streakQuest.CurrentValue = currentStreak;
                }

                inLevel = false;
                notPipe = true;

                while (_ram.ReadUint8(RamBaseAddress + 0x14) == 1);
            }

            _levelQuest.UpdateValue(levelsCleared);
            _streakQuest.UpdateValue(currentStreak);
        }
    }
}
