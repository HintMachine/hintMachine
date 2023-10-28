using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class SuperMonkeyBallConnector : IDolphinConnector
    {
        private readonly HintQuestCumulative _bananaQuest = new HintQuestCumulative
        {
            Name = "Bananas collected",
            GoalValue = 50,
	    MaxIncrease = 10
        };

        private readonly HintQuestCumulative _playPointsQuest = new HintQuestCumulative
        {
            Name = "Play Points",
            GoalValue = 250
        };

        public SuperMonkeyBallConnector() : base(false)
        {
            Name = "Super Monkey Ball";
            Description = "Conceived by Amusement Vision head Toshihiro Nagoshi, " +
                "Super Monkey Ball involves guiding a transparent ball containing one of four " +
                "monkeys—AiAi, MeeMee, Baby, and GonGon—across a series of maze-like platforms. " +
                "The player must reach the goal without falling off or letting the timer reach zero " +
                "to advance to the next stage. There are also several multiplayer modes: independent " +
                "minigames as well as extensions of the main single-player game. " +
                "Quests work on all difficulties of the main game.";
            SupportedVersions.Add("NTSC US");
            CoverFilename = "super_monkey_ball.png";
            Author = "Spicynun";

            Quests.Add(_bananaQuest);
            Quests.Add(_playPointsQuest);

            ValidROMs.Add("GMBE8P");
        }

        protected override bool Poll()
        {
            // Banana counts
            int bananaCount = _ram.ReadUint8(MEM1 + 0x205EDB);
            _bananaQuest.UpdateValue(bananaCount);


            // Play Points
            uint playPoints = _ram.ReadUint32(MEM1 + 0x2f1fc4);
            _playPointsQuest.UpdateValue(playPoints);

            return true;
        }
    }
}
