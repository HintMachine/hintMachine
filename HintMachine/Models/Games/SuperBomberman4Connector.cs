using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class SuperBomberman4Connector : ISNESConnector
    {
        private readonly HintQuestCounter _storyLevelsQuest = new HintQuestCounter {
            Name = "Levels Cleared (Story mode)",
            GoalValue = 3,
            MaxIncrease = 1
        };

        private readonly HintQuestCounter _bossesBeatenQuest = new HintQuestCounter {
            Name = "Bosses Beaten (Story mode)",
            Description = "Final boss counts double",
            GoalValue = 1,
            MaxIncrease = 1
        };

        private short _highestLevel = 0;

        // --------------------------------------------------

        public SuperBomberman4Connector()
        {
            Name = "Super Bomberman 4";
            Description = "The fourth SNES game in Hudson's explosive multiplayer series sends White Bomber and Black Bomber hurtling through time.";
            SupportedVersions.Add("NTSC-J (🇯🇵)");
            SupportedVersions.Add("NTSC-J English Translation");
            CoverFilename = "super_bomberman_4.png";
            Author = "Dinopony";

            Quests.Add(_storyLevelsQuest);
            Quests.Add(_bossesBeatenQuest);
            ValidROMs.Add("EE61EE2DAB6400599B59BF93FADEA7242A052A1D256C5B185327B778A84FE672");
            ValidROMs.Add("D57074B53D004FFE2A2E3415CB6B8098DE92E7373FBACBF5C067423A62E88FC7");
        }

        protected override bool Poll()
        {
            bool isStoryMode = (_ram.ReadUint8(RamBaseAddress + 0x12089) == 1);

            // Track maximal value for "current level": when this value increases by exactly one, +1 to a “completed levels” quest
            short currentLevel = _ram.ReadUint8(RamBaseAddress + 0x92);
            if (currentLevel == _highestLevel + 1 && isStoryMode)
            {
                _storyLevelsQuest.CurrentValue += 1;
                _highestLevel = currentLevel;

                // Bosses for world 1-4
                if(currentLevel == 8 || currentLevel == 16 || currentLevel == 24 || currentLevel == 32)
                    _bossesBeatenQuest.CurrentValue += 1;
            }

            // If this specific combination of parameters happen, this means we just beat the final boss
            if (_ram.ReadUint8(RamBaseAddress + 0x1F14) == 0 && _highestLevel == 37)
            {
                // We perform two subsequent increases to circumvent the MaxIncrease being set to 1
                _bossesBeatenQuest.CurrentValue += 1;
                _bossesBeatenQuest.CurrentValue += 1;
                _highestLevel = 38;
            }

            return true;
        }
    }
}
