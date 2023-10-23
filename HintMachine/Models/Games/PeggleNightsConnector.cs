using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    public class PeggleNightsConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "popcapgame1",
            // We can't use hash checking for this connector. The popcapgame1 executable is created on the fly by PeggleNights.exe every time the game launches and we don't have the proper permissions to interact with it
        };

        private readonly HintQuestCumulative _orangeQuest = new HintQuestCumulative
        {
            Name = "Orange Pegs Cleared",
            GoalValue = 50,
            MaxIncrease = 10,
        };

        private readonly HintQuestCumulative _greenQuest = new HintQuestCumulative
        {
            Name = "Green Pegs Cleared",
            GoalValue = 12,
        };

        private readonly HintQuestCumulative _totalQuest = new HintQuestCumulative
        {
            Name = "Total Pegs Cleared",
            GoalValue = 300,
            MaxIncrease = 30,
        };

        private ProcessRamWatcher _ram = null;

        public PeggleNightsConnector()
        {
            Name = "Peggle Nights";
            Description = "The sun has set at the Peggle Institute, but the bouncy delight has just begun! Join the Peggle Masters on a dreamtime adventure of alter egos and peg-tastic action. Stay up late to aim, shoot and clear orange pegs, and bask in Extreme Fever glory under the silver moon.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "peggle_nights.png";
            Author = "Serpent.AI";

            Quests.Add(_orangeQuest);
            Quests.Add(_greenQuest);
            Quests.Add(_totalQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(GAME_VERSION_STEAM);
            _ram.Is64Bit = false;

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            // Make sure popcapgame1.exe is actually Peggle Nights and not a different PopCap game
            byte[] signature = _ram.ReadBytes(_ram.BaseAddress + 0x2CA09A, 6);
            string signatureString = System.Text.Encoding.UTF8.GetString(signature, 0, signature.Length);

            if (signatureString != "Nights") { return false; }

            long logicManagerStructAddress = _ram.ResolvePointerPath32(_ram.Threadstack0 - 0x2A4, new int[] { 0x20, 0x0, 0x8, 0x104, 0x698, 0x0 });

            if (logicManagerStructAddress != 0)
            {
                try
                {
                    long orangeValue = _ram.ReadUint32(logicManagerStructAddress + 0x4C);
                    long greenValue = _ram.ReadUint32(logicManagerStructAddress + 0x50);
                    long totalValue = _ram.ReadUint32(logicManagerStructAddress + 0x1B0);

                    _orangeQuest.UpdateValue(orangeValue);
                    _greenQuest.UpdateValue(greenValue);
                    _totalQuest.UpdateValue(totalValue);
                }
                catch { }
            }

            return true;
        }
    }
}
