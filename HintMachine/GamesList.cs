using HintMachine.Games;
using System.Collections.Generic;

namespace HintMachine
{
    public abstract class GamesList
    {
        public static List<IGameConnector> GAMES = new List<IGameConnector>()
        {
            new XenotiltConnector(),
            new OneFingerDeathPunchConnector(),
            new PuyoTetrisConnector(),
            new TetrisEffectConnector(),
            new ZachtronicsSolitaireConnector(),
            new GeometryWarsGalaxiesConnector(),
        };
        
        public static IGameConnector FindGameFromName(string name)
        {
            foreach (IGameConnector game in GAMES)
                if (game.GetDisplayName() == name)
                    return game;

            return null;
        }
    }
}
