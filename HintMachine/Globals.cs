
using HintMachine.Games;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HintMachine
{
    internal static class Globals
    {
        public const string ProgramName = "HintMachine";
        public const string ProgramVersion = "1.1.0";

        /// <summary>
        /// The duration of a "tick" where the current game connector (if any) watches the game once, in milliseconds
        /// </summary>
        public const int TickInterval = 100;

        /// <summary>
        /// The minimal duration between two hint queries obtained from quests
        /// </summary>
        public const int HintQueueInterval = 1000;

        /// <summary>
        /// The maximal amount of pending hints obtained from quest that can be stored at any given time.
        /// If more hints are obtained, they will be discarded (but it will most likely be because of a bug / exploit anyway)
        /// </summary>
        public const int PendingHintsQueueMaxSize = 5;

        public static readonly string NotificationSoundPath = 
            $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Assets\Notification.wav";

        public static readonly List<IGameConnector> Games = new List<IGameConnector>()
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
            new FZeroGXConnector(),
            new IslandersConnector(),
            new DorfromantikConnector(),
            new MeteosConnector(),
            new PacManChampionshipEditionDXConnector(),
            new MetroidPrimePinballConnector(),
            new ColumnsConnector(),
            new BPMConnector(),
            new MiniMetroConnector(),
            new LuckBeALandlordConnector(),
            new SuperHexagonConnector(),
            new SanrioWorldSmashBallConnector(),
        };

        public static IGameConnector FindGameFromName(string name)
        {
            foreach (IGameConnector game in Games)
                if (game.Name == name)
                    return game;

            return null;
        }

        // Easter egg commands
        public static readonly List<string> HitMachineFacts = new List<string>() {
            "Le saviez vous ? Avant d'être présenté par Charly et Lulu, le Hit Machine était animé par Ophelie Winter et Yves Noel.",
            "Le saviez vous ? Le Hit Machine a été diffusé sur M6 entre 1994 et 2009",
            "Le saviez vous ? Le developpement de la HintMachine a demarré en Septembre 2023.",
            "✨ Je m'appelle Charly - Et je m'appelle Lulu - On est sur M6 - pour le HitMachine ✨",

        };
        public static readonly List<string> CharlyMachineFacts = new List<string>() {
            "Le saviez vous ? Charly se nomme Charly Nestor.",
            "Le saviez vous ? Charly est né le 10 avril 1964",
            "Le feu ça brule"
        };
        public static readonly List<string> LuluMachineFacts = new List<string>() {
            "Le saviez vous ? Lulu se nomme Jean-Marc Lubin",
            "Le saviez vous ? Lulu a commencé sa carriere télé en 1990",
            "L'eau ça mouille"
        };
    }
}
