using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    internal class SuperMarioBros2Connector : INESConnector
    {
        private int currentLives = 0;
        private int currentWorld = 0;
        private int currentCherries = 0;
        private int currentCoins = 0;
        private int marioClears = 0;
        private int luigiClears = 0;
        private int toadClears = 0;
        private int peachClears = 0;

        private readonly HintQuestCounter _streakQuest = new HintQuestCounter
        {
            Name = "Streak",
            GoalValue = 3,
            MaxIncrease = 1,
        };

        private readonly HintQuestCounter _worldQuest = new HintQuestCounter
        {
            Name = "Worlds",
            GoalValue = 1,
            MaxIncrease = 1,
        };

        private readonly HintQuestCounter _cherryQuest = new HintQuestCounter
        {
            Name = "Cherries",
            GoalValue = 15,
            MaxIncrease = 1,
        };

        private readonly HintQuestCounter _coinQuest = new HintQuestCounter
        {
            Name = "Coins",
            GoalValue = 10,
            MaxIncrease = 1,
        };

        private readonly HintQuestSingle _marioQuest = new HintQuestSingle
        {
            Name = "Mario",
            GoalValue = 5,
            MaxIncrease = 1,
        };

        private readonly HintQuestSingle _luigiQuest = new HintQuestSingle
        {
            Name = "Luigi",
            GoalValue = 5,
            MaxIncrease = 1,
        };

        private readonly HintQuestSingle _toadQuest = new HintQuestSingle
        {
            Name = "Toad",
            GoalValue = 5,
            MaxIncrease = 1,
        };

        private readonly HintQuestSingle _peachQuest = new HintQuestSingle
        {
            Name = "Peach",
            GoalValue = 5,
            MaxIncrease = 1,
        };

        public SuperMarioBros2Connector()
        {
            Name = "Super Mario Bros. 2";
            Description = "Mario has a strange dream in which he finds a mysterious door before he awakes."
                + " He and his friends decide to take a picnic and eventually stumble upon a cave which"
                + " they decide to explore. With the cave, they eventually come across a door...the very"
                + " same door Mario had just seen within his dream!? What will Mario and friends find"
                + " beyond that door and will they ever return?";
            SupportedVersions.Add("(USA) (Rev1)");
            CoverFilename = "super_mario_bros_2.png";
            Author = "RadzPrower";

            Quests.Add(_streakQuest);
            Quests.Add(_worldQuest);
            Quests.Add(_cherryQuest);
            Quests.Add(_coinQuest);
            Quests.Add(_marioQuest);
            Quests.Add(_luigiQuest);
            Quests.Add(_toadQuest);
            Quests.Add(_peachQuest);

            ValidROMs.Add("6CA47E9DA206914730895E45FEF4F7393E59772C1C80E9B9BEFC1A01D7ECF724");
        }

        protected override bool Poll()
        {
            CheckLives();
            CheckLevelClears();
            CheckWorld();
            CheckCherries();
            CheckCoins();

            return true;
        }

        private void CheckLives()
        {
            // Pull the current number of lives from RAM
            int livesInRAM = _ram.ReadUint8(RamBaseAddress + 0x4ed);

            // Reset streak counter if the lives pulled from RAM is less than the lives count on file
            if (livesInRAM < currentLives)
            {
                _streakQuest.CurrentValue = 0;
                currentCherries = 0;
            }

            currentLives = livesInRAM;
        }

        private void CheckLevelClears()
        {
            // Process Mario level clear count
            _marioQuest.CurrentValue = CheckCharacterLevels(_marioQuest, ref marioClears, _ram.ReadUint8(RamBaseAddress + 0x62d));

            // Process Luigi level clear count
            _luigiQuest.CurrentValue = CheckCharacterLevels(_luigiQuest, ref luigiClears, _ram.ReadUint8(RamBaseAddress + 0x630));

            // Process Toad level clear count
            _toadQuest.CurrentValue = CheckCharacterLevels(_toadQuest, ref toadClears, _ram.ReadUint8(RamBaseAddress + 0x62f));

            // Process Peach level clear count
            _peachQuest.CurrentValue = CheckCharacterLevels(_peachQuest, ref peachClears, _ram.ReadUint8(RamBaseAddress + 0x62e));
        }

        private long CheckCharacterLevels(HintQuestSingle _quest, ref int clears, byte levelCount)
        {
            long current = _quest.CurrentValue;

            // Only increment character quests if 5 or less levels cleared,
            // otherwise the count should remain at zero.
            if (clears < 5)
            {
                if (levelCount > clears)
                {
                    current++;
                }
                else if (levelCount == 5 && clears == 4)
                {
                    current++;
                }
                else if (levelCount != clears) current = 0;
            }

            // Increment streak and level counts for character and clear the coins if the level is cleared
            if (levelCount > clears)
            {
                _streakQuest.CurrentValue++;
                clears = levelCount;
            }

            return current;
        }

        private void CheckWorld()
        {
            // Pull the current world from RAM
            int worldInRAM = _ram.ReadUint8(RamBaseAddress + 0x635);

            // Only increment the world quest if moving one world ahead to avoid giving credit for warps
            if (worldInRAM == currentWorld + 1)
            {
                _worldQuest.CurrentValue++;
            }

            // Update our world on file
            currentWorld = worldInRAM;
        }

        private void CheckCherries()
        {
            // Pull current number of cherries in RAM
            int cherriesInRAM = _ram.ReadUint8(RamBaseAddress + 0x62a);

            // If RAM value is higher than our current value, increment counter
            if (cherriesInRAM > currentCherries)
            {
                _cherryQuest.CurrentValue++;
            }
            // If value in RAM is zero due to rollover from collecting 5 cherries,
            // check if our current cherry count is 4 implying the value was rolled over
            else if (cherriesInRAM == 0 && currentCherries == 4)
            {
                _cherryQuest.CurrentValue++;
            }

            // Update our current cherry count on file
            currentCherries = cherriesInRAM;
        }

        private void CheckCoins()
        {
            // Pull coin count from RAM
            int coinsInRAM = _ram.ReadUint8(RamBaseAddress + 0x62b);

            // If new coin value is larger than last, increment coin counter and update our coin count on file
            if (coinsInRAM > currentCoins)
            {
                _coinQuest.CurrentValue++;
            }
            else if (coinsInRAM < currentCoins)
            {
                currentCoins = coinsInRAM;
            }

            // Update coin count on file
            currentCoins = coinsInRAM;
        }
    }
}
