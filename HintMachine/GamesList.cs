using HintMachine.Games;

namespace HintMachine
{
    public abstract class GamesList
    {
        public static IGameConnector[] GAMES =
        {
            new XenotiltConnector(),
            new OneFingerDeathPunchConnector(),
            new PuyoTetrisConnector(),
            new TetrisEffectConnector()
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
