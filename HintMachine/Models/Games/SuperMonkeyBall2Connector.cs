using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class SuperMonkeyBall2Connector : IDolphinConnector
    {
        private readonly HintQuestCumulative _bananaQuest = new HintQuestCumulative
        {
            Name = "Bananas collected",
            GoalValue = 50,
	    MaxIncrease = 10
        };

        private readonly HintQuestCumulative _stageQuest = new HintQuestCumulative
        {
            Name = "Stages cleared",
            GoalValue = 3,
            MaxIncrease = 1
        };

        public SuperMonkeyBall2Connector() : base(false)
        {
            Name = "Super Monkey Ball 2";
            Description = "Traverse through stages filled to the brim with obstacles and collect bananas " +
                          "along the way as a monkey inside of a ball! Quests work on story mode and all difficulties of challenge mode.";
            SupportedVersions.Add("NTSC US");
            CoverFilename = "super_monkey_ball_2.png";
            Author = "Spicynun";
            
            Quests.Add(_bananaQuest);
            Quests.Add(_stageQuest);
            
            ValidROMs.Add("GM2E8P");
        }
   
        protected override bool Poll()
        {
            // Banana counts for story and challenge
            int player1banana = _ram.ReadUint8(MEM1 + 0x5bca1b); 
            int bananaCount = player1banana;
            _bananaQuest.UpdateValue(bananaCount);

            // Story mode world clears
            int world1 = _ram.ReadUint8(MEM1 + 0x5d4b0b);
            int world2 = _ram.ReadUint8(MEM1 + 0x5d4b43);
            int world3 = _ram.ReadUint8(MEM1 + 0x5d4b7b);
            int world4 = _ram.ReadUint8(MEM1 + 0x5d4bb3);
            int world5 = _ram.ReadUint8(MEM1 + 0x5d4beb);
            int world6 = _ram.ReadUint8(MEM1 + 0x5d4c23);
            int world7 = _ram.ReadUint8(MEM1 + 0x5d4c5b);
            int world8 = _ram.ReadUint8(MEM1 + 0x5d4c93);
            int world9 = _ram.ReadUint8(MEM1 + 0x5d4ccb);
            int world10 = _ram.ReadUint8(MEM1 + 0x5d4d03);

            // Challenge mode clears
            int player1challenge = _ram.ReadUint8(MEM1 + 0x5d4973);

            // Checks if story menu is accessed which sets progress to 0
            int inStoryMenu = _ram.ReadUint8(MEM1 + 0x54dbef);

            // Prevent the stage clear from increasing every time a save in story mode is loaded
            if (inStoryMenu != 0)
            {
                int stageClears = world1 + world2 + world3 + world4 + world5 + world6 + world7 + world8 + world9 + world10 + player1challenge;
                _stageQuest.UpdateValue(stageClears);
            }

            return true;
        }
    }
}
