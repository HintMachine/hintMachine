using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class SanrioWorldSmashBallConnector : ISNIConnector
    {
        private readonly HintQuestCumulative _gamesQuest = new HintQuestCumulative
        {
            Name = "Win Matches!",
            GoalValue = 5,
            CooldownBetweenIncrements = 30, // believe it or not, sub-minute is possible
            MaxIncrease = 1
        };
        private readonly HintQuestCumulative _goalQuest = new HintQuestCumulative
        {
            Name = "Score Goals!",
            GoalValue = 10,
            MaxIncrease = 1,
        };
        private readonly HintQuestCumulative _superQuest = new HintQuestCumulative
        {
            Name = "Build Super Meter!",
            GoalValue = 0xA0 * 20,
        };
        private readonly HintQuestCounter _powerupQuest = new HintQuestCounter
        {
            Name = "Pick Up Powerups!",
            GoalValue = 10,
            MaxIncrease = 1,
        };
        private byte _pw1 = 255;
        private byte _pw2 = 255;
        public SanrioWorldSmashBallConnector() {
            Name = "Sanrio World Smash Ball!";
            Description = 
                "Play as Keroppi, Tābō, Pokopon, and Hangyodon in a thrilling table tennis-like game of Smash Ball!!\n\n" +
                "As the ball moves around the field, players can hit the ball, acquire powerups, and build up their super move!\n\n" +
                "Play through 30 stages within the game's singleplayer mode, or continue where you left off using a password!";
            SupportedVersions.Add("Japanese");
            CoverFilename = "sanrio_world_smash_ball.png";
            RomName = "SANRIO SMASH BALL!";
            Author = "Silvris";
            Quests.Add(_gamesQuest);
            Quests.Add(_goalQuest);
            Quests.Add(_superQuest);
            Quests.Add(_powerupQuest);
        }

        protected override bool Poll()
        {
            if (!ConfirmRomName())
            {
                return false;
            }
            byte currentStage = ReadByte(0xF50055, SNI.AddressSpace.FxPakPro);
            if (currentStage < 0x3e) // Multiplayer, ignore for this quest
            {
                _gamesQuest.UpdateValue(currentStage);
                byte currentScore = ReadByte(0xF5005D, SNI.AddressSpace.FxPakPro);
                _goalQuest.UpdateValue(currentScore);
                byte currentSuper = ReadByte(0xF50070, SNI.AddressSpace.FxPakPro);
                _superQuest.UpdateValue(currentSuper);
                byte pw1 = ReadByte(0xF5018D, SNI.AddressSpace.FxPakPro);
                if (pw1 > _pw1)
                {
                    _powerupQuest.CurrentValue++;
                }
                _pw1 = pw1;
                byte pw2 = ReadByte(0xF5018E, SNI.AddressSpace.FxPakPro);
                if (pw2 > _pw2)
                {
                    _powerupQuest.CurrentValue++;
                }
                _pw2 = pw2;

            }
            return true;
        }
    }
}
