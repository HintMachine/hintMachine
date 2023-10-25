using HintMachine.Models.GenericConnectors;
using System;

namespace HintMachine.Models.Games
{
    internal class SuperMarioBros2Connector : INESConnector
    {
        private readonly HintQuestCumulative _streakQuest = new HintQuestCumulative
        {
            Name = "Streak",
            GoalValue = 5,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _worldQuest = new HintQuestCumulative
        {
            Name = "Worlds",
            GoalValue = 1,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _cherryQuest = new HintQuestCumulative
        {
            Name = "Cherries",
            GoalValue = 15,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _coinQuest = new HintQuestCumulative
        {
            Name = "Coins",
            GoalValue = 20,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _marioQuest = new HintQuestCumulative
        {
            Name = "Mario",
            GoalValue = 5,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _LuigiQuest = new HintQuestCumulative
        {
            Name = "Luigi",
            GoalValue = 5,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _peachQuest = new HintQuestCumulative
        {
            Name = "Peach",
            GoalValue = 5,
            MaxIncrease = 1,
        };

        private readonly HintQuestCumulative _toadQuest = new HintQuestCumulative
        {
            Name = "Toad",
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
            Quests.Add(_LuigiQuest);
            Quests.Add(_peachQuest);
            Quests.Add(_toadQuest);

            ValidROMs.Add("6CA47E9DA206914730895E45FEF4F7393E59772C1C80E9B9BEFC1A01D7ECF724");
        }

        protected override bool Poll()
        {
            CheckLives();
            CheckLevelClears();
            CheckWorld();

            return true;
        }

        private void CheckLives()
        {
            throw new NotImplementedException();
        }

        private void CheckLevelClears()
        {
            throw new NotImplementedException();
        }

        private void CheckWorld()
        {
            throw new NotImplementedException();
        }
    }
}
