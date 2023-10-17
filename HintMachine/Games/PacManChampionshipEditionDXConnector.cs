namespace HintMachine.Games
{
    public class PacManChampionshipEditionDXConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "PAC-MAN",
            Hash = "B96AAB05C2A3E767FDE271A14A0052915D89418F000F5BDE75B74777608721F1"
        };

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 400000,
        };

        private ProcessRamWatcher _ram = null;

        public PacManChampionshipEditionDXConnector()
        {
            Name = "PAC-MAN Championship Edition DX+";
            Description = "The sequel to the original PAC-MAN and award-winning power pellet chomping game, PAC-MAN Championship Edition DX returns with even more content! Chomp through bright neon mazes at blistering speeds to unlock brand new achievements and medals for an increased challenge! With a refined user-interface, it's easier than ever to compare high scores with your friends! Get ready for more ghost chain gobbling and frantic action in PAC-MAN CE-DX+";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "pacman_cedx.png";
            Author = "Serpent.AI";

            Quests.Add(_scoreQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher();
            _ram.SupportedTargets.Add(GAME_VERSION_STEAM);

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            _scoreQuest.UpdateValue(_ram.ReadUint32(_ram.BaseAddress + 0x33CA14));
            return true;
        }
    }
}