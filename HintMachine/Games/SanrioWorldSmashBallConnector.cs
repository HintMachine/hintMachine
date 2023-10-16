using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class SanrioWorldSmashBallConnector : ISNIConnector
    {
        private readonly HintQuestCounter _gamesQuest = new HintQuestCounter 
        {
            Name = "Win Matches!",
            GoalValue = 2,
            CooldownBetweenIncrements = 30 // believe it or not, sub-minute is possible
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
            GoalValue = 0xA0 * 5,
            //we'll have to use our judgement for max increase
        };
        private readonly HintQuestCumulative _powerupQuest = new HintQuestCumulative
        {
            Name = "Pick Up Powerups!",
            GoalValue = 30,
            MaxIncrease = 1,
        };
        private byte _previousStage = 255;
        private byte _previousScore = 255;
        private byte _previousSuper = 255;
        private byte _pw1 = 255;
        private byte _pw2 = 255;
        public SanrioWorldSmashBallConnector() {
            Name = "Sanrio World Smash Ball!";
            Description = "Play as Keroppi, Tābō, Pokopon, and Hangyodon in a thrilling table tennis-like game of Smash Ball!!\n\n ";
            SupportedVersions.Add("Japanese");
            CoverFilename = "sanrio_world_smash_ball.png";
            RomName = "SANRIO SMASH BALL!";
            Author = "Silvris";
            Quests.Add(_gamesQuest);
            Quests.Add(_goalQuest);
            Quests.Add(_superQuest);
            Quests.Add(_powerupQuest);
        }

        public override bool Poll()
        {
            if (!ConfirmRomName())
            {
                return false;
            }
            byte currentStage = ReadByte(0xF50055, SNI.AddressSpace.FxPakPro);
            if (currentStage < 0x3e) // Multiplayer, ignore for this quest
            {
                if (currentStage > _previousStage)
                {
                    _gamesQuest.CurrentValue++;
                    
                }
                _previousStage = currentStage;
                byte currentScore = ReadByte(0xF5005D, SNI.AddressSpace.FxPakPro);
                if (currentScore > _previousScore)
                {
                    _goalQuest.CurrentValue++;
                }
                _previousScore = currentScore;
                byte currentSuper = ReadByte(0xF50070, SNI.AddressSpace.FxPakPro);
                if (currentSuper > _previousSuper)
                {
                    _superQuest.CurrentValue = _superQuest.CurrentValue + (currentSuper - _previousSuper);
                }
                _previousSuper = currentSuper;
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
