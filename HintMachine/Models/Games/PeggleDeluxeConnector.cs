using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class PeggleDeluxeConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "popcapgame1",
            // We can't use hash checking for this connector. The popcapgame1 executable is created on the fly by Peggle.exe every time the game launches and we don't have the proper permissions to interact with it
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

        public PeggleDeluxeConnector()
        {
            Name = "Peggle Deluxe";
            Description = "Take your best shot with energizing arcade fun! Aim, shoot, clear the orange pegs, then sit back and cheer as 10 whimsical teachers guide you to Peggle greatness. Conquer 55 fanciful levels with 10 mystical Magic Powers, racking up bonus points and shots you'll smile about for weeks.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "peggle_deluxe.png";
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
            // Make sure popcapgame1.exe is actually Peggle and not a different PopCap game
            byte[] signature = _ram.ReadBytes(_ram.BaseAddress + 0x288BCC, 6);
            string signatureString = System.Text.Encoding.UTF8.GetString(signature, 0, signature.Length);

            if (signatureString != "Peggle") { return false; }

            long logicManagerStructAddress = _ram.ResolvePointerPath32(_ram.Threadstack0 - 0x268, new int[] { 0x1C, 0x150, 0x0 });

            if (logicManagerStructAddress != 0)
            {
                try
                {
                    long orangeValue = _ram.ReadUint32(logicManagerStructAddress + 0x4C);
                    long greenValue = _ram.ReadUint32(logicManagerStructAddress + 0x50);
                    long totalValue = _ram.ReadUint32(logicManagerStructAddress + 0x12C);

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
