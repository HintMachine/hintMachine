using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class TetrisNESConnector : INESConnector
    {
        private readonly HintQuestCumulative _linesQuest = new HintQuestCumulative
        {
            Name = "Cleared Lines",
            GoalValue = 40,
        };
        
        public TetrisNESConnector()
        {
            Name = "Tetris";
            Description = "Tetris is a tile-matching puzzle video game. The goal is to place pieces made up of four tiles in a ten-by-twenty well, organizing them into complete rows, which then disappear. The main objective of each round is to clear 25 lines, after which the player moves on to the next round. If the stack reaches the top of the field, the player loses a life, and if all three lives are lost, the game is over.\r\n\r\nThe game lets the player choose the starting stage and round, as well as one of three background tunes. Difficulty is increased throughout the stages by an increase in speed and the addition of garbage blocks in the well.";
            SupportedVersions.Add("PAL (🇪🇺)");
            SupportedVersions.Add("NTSC-U (🇺🇸)");
            CoverFilename = "tetris_nes.png";
            Author = "Dinopony";

            Quests.Add(_linesQuest);

            ValidROMs.Add("8DA063C3A4D9DB281B0F7E32DC9EFAC3CA505BB97845B6ECA2AF525960EB51A0");
            ValidROMs.Add("42CD2FF75AD808D7444FEEB64009DBAC817561BE807418B1E26355BB21254280");
        }
        
        protected override bool Poll()
        {
            int lowestTwoDigits = _ram.ReadUint8(RamBaseAddress + 0x50);
            int highestTwoDigits = _ram.ReadUint8(RamBaseAddress + 0x51);

            int[] digits = new int[4];
            digits[3] = (highestTwoDigits >> 4);
            digits[2] = (highestTwoDigits & 0x0F);
            digits[1] = (lowestTwoDigits >> 4);
            digits[0] = (lowestTwoDigits & 0x0F);

            int lines = 0;
            int currentPowerOf10 = 1;
            for (var i = 0; i < digits.Length; ++i)
            {
                lines += digits[i] * currentPowerOf10;
                currentPowerOf10 *= 10;
            }
            _linesQuest.UpdateValue(lines);

            return true;
        }
    }
}
