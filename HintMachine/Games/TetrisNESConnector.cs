﻿using HintMachine.GenericConnectors;
using System;
using System.Runtime.Remoting;

namespace HintMachine.Games
{
    public class TetrisNESConnector : INESConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 10000,
        };
        
        public TetrisNESConnector()
        {
            Name = "Tetris";
            Description = "Tetris is a tile-matching puzzle video game. The goal is to place pieces made up of four tiles in a ten-by-twenty well, organizing them into complete rows, which then disappear. The main objective of each round is to clear 25 lines, after which the player moves on to the next round. If the stack reaches the top of the field, the player loses a life, and if all three lives are lost, the game is over.\r\n\r\nThe game lets the player choose the starting stage and round, as well as one of three background tunes. Difficulty is increased throughout the stages by an increase in speed and the addition of garbage blocks in the well.";
            SupportedVersions.Add("PAL (🇪🇺)");
            SupportedVersions.Add("NTSC-U (🇺🇸)");
            CoverFilename = "tetris_nes.png";
            Author = "Dinopony";

            Quests.Add(_scoreQuest);

            ValidROMs.Add("8DA063C3A4D9DB281B0F7E32DC9EFAC3CA505BB97845B6ECA2AF525960EB51A0");
            ValidROMs.Add("42CD2FF75AD808D7444FEEB64009DBAC817561BE807418B1E26355BB21254280");
        }
        
        protected override bool Poll()
        {
            int lowestTwoDigits = _ram.ReadUint8(RamBaseAddress + 0x53);
            int middleTwoDigits = _ram.ReadUint8(RamBaseAddress + 0x54);
            int highestTwoDigits = _ram.ReadUint8(RamBaseAddress + 0x55);

            int[] scoreDigits = new int[6];
            scoreDigits[5] = (highestTwoDigits >> 4);
            scoreDigits[4] = (highestTwoDigits & 0x0F);
            scoreDigits[3] = (middleTwoDigits >> 4);
            scoreDigits[2] = (middleTwoDigits & 0x0F);
            scoreDigits[1] = (lowestTwoDigits >> 4);
            scoreDigits[0] = (lowestTwoDigits & 0x0F);

            int score = 0;
            int currentPowerOf10 = 1;
            for (var i = 0; i < scoreDigits.Length; ++i)
            {
                score += scoreDigits[i] * currentPowerOf10;
                currentPowerOf10 *= 10;
            }
            _scoreQuest.UpdateValue(score);

            return true;
        }
    }
}
