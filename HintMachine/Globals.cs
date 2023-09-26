
using HintMachine.Games;
using System.Collections.Generic;

namespace HintMachine
{
    internal static class Globals
    {
        public const string ProgramName = "HintMachine";
        public const string ProgramVersion = "1.0";

        public static List<IGameConnector> Games = new List<IGameConnector>()
        {
            new XenotiltConnector(),
            new OneFingerDeathPunchConnector(),
            new PuyoTetrisConnector(),
            new TetrisEffectConnector(),
            new ZachtronicsSolitaireConnector(),
            new GeometryWarsConnector(),
            new GeometryWarsGalaxiesConnector(),
            //new NuclearThroneConnector(), //Instabilities, must be investigated
            new SonicBlueSpheresConnector(),
            new StargunnerConnector(),
            new BustAMove4Connector(),
            new Rollcage2Connector(),
        };

        public static IGameConnector FindGameFromName(string name)
        {
            foreach (IGameConnector game in Games)
                if (game.Name == name)
                    return game;

            return null;
        }
    }
}
