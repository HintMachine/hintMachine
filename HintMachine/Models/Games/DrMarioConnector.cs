using HintMachine.Models.GenericConnectors;
using static HintMachine.Models.HintQuestCumulative;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class DrMarioConnector : INESConnector
    {
        private readonly HintQuestCumulative _virusQuest = new HintQuestCumulative
        {
            Name = "Viruses Destroyed",
            GoalValue = 30,
            Direction = CumulativeDirection.DESCENDING,
            MaxIncrease = 3
        };

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 15000,
            MaxIncrease = 2000
        };

        public DrMarioConnector()
        {
            Name = "Dr. Mario";
            Description = "Dr. Mario is a falling block tile-matching video game, in which Mario assumes the role of a doctor, dropping two-colored medical capsules into a medicine bottle representing the playing field. This area is populated by viruses of three colors: red, yellow, and blue. In a manner and style considered similar to Tetris, the player manipulates each capsule as it falls, moving it left or right and rotating it such that it is positioned alongside the viruses and any existing capsules. When four or more capsule halves or viruses of matching color are aligned in vertical or horizontal configurations, they are removed from play. The main objective is to complete levels, which is accomplished by eliminating all viruses from the playing field. A game over occurs if capsules fill up the playing field in a way that obstructs the bottle's narrow neck.";
            
            SupportedVersions.Add("NTSC (US/JP)");
            SupportedVersions.Add("NTSC (US/JP) - Rev A");
            SupportedVersions.Add("PAL (EU)");
            
            CoverFilename = "dr_mario.png";
            Author = "Serpent.AI";

            Quests.Add(_virusQuest);
            Quests.Add(_scoreQuest);

            ValidROMs.Add("DB97089822667CB5BEFE91EBC9E5C72BE04383D60E6DBCA9531EF2BA62187382");
            ValidROMs.Add("5F4849329F0ECE227A3C7D6A63C0ECB6C9A1EDEE424E70E7BC6FA4BFDA4A82D8");
            ValidROMs.Add("4CBEFF53601AB83D60FA6DAD3F535CA4DE3C0E8CB923A7B7235A3E39B5D41719");
        }
        
        protected override bool Poll()
        {
            int musicSelection = _ram.ReadUint8(RamBaseAddress + 0x731);
            int players = _ram.ReadUint8(RamBaseAddress + 0x727);
            int gameState = _ram.ReadUint8(RamBaseAddress + 0x46);

            // Music selection byte gets set to 3 exclusively when the demo is playing
            // We exclude the demo and 2 player mode from the quest tracking
            // We also need to be playing a stage
            if (musicSelection < 3 && players == 1 && gameState == 4)
            {
                // Viruses Destroyed
                int virusesYellowRemaining = _ram.ReadUint8(RamBaseAddress + 0x72);
                int virusesRedRemaining = _ram.ReadUint8(RamBaseAddress + 0x73);
                int virusesBlueRemaining = _ram.ReadUint8(RamBaseAddress + 0x74);

                _virusQuest.UpdateValue(virusesYellowRemaining + virusesRedRemaining + virusesBlueRemaining);

                // Score
                int[] scoreDigits = new int[7];

                scoreDigits[0] = _ram.ReadUint8(RamBaseAddress + 0x72E);
                scoreDigits[1] = _ram.ReadUint8(RamBaseAddress + 0x72D);
                scoreDigits[2] = _ram.ReadUint8(RamBaseAddress + 0x72C);
                scoreDigits[3] = _ram.ReadUint8(RamBaseAddress + 0x72B);
                scoreDigits[4] = _ram.ReadUint8(RamBaseAddress + 0x72A);
                scoreDigits[5] = _ram.ReadUint8(RamBaseAddress + 0x729);
                scoreDigits[6] = _ram.ReadUint8(RamBaseAddress + 0x728);

                int score = 0;

                foreach (int digit in scoreDigits)
                {
                    score = score * 10 + (digit >> 4) * 10 + (digit & 0x0F);
                }

                _scoreQuest.UpdateValue(score);
            }
            else
            {
                _scoreQuest.IgnoreNextValue();
                _virusQuest.IgnoreNextValue();
            }

            return true;
        }
    }
}
