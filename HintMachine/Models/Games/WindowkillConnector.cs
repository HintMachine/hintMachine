using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class WindowkillConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_ITCH = new BinaryTarget
        {
            DisplayName = "itch.io",
            ProcessName = "windowkill",
            Hash = "6670AA7B50CAF97CF07205907BC19A7FD233E17B6B55A2177B98941CB8101AB0"
        };

        private readonly HintQuestCumulative _coinsQuest = new HintQuestCumulative
        {
            Name = "Coins Collected",
            GoalValue = 200,
            MaxIncrease = 30,
        };

        private ProcessRamWatcher _ram = null;

        public WindowkillConnector()
        {
            Name = "Windowkill";
            Description = "Welcome to windowkill.\nIt's just your normal shoot-em-up, but something's wrong with the window....\nOnly works with version 1.6 of Windowkill.";
            Platform = "PC";
            SupportedVersions.Add("itch.io");
            CoverFilename = "windowkill.png";
            Author = "Chandler";

            Quests.Add(_coinsQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(GAME_VERSION_ITCH);
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            if (!_ram.TestProcess())
                return false;

            try {
                long coinAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x344FFC0, new int[] { 0x348, 0x1C0, 0x10, 0x68, 0x28, 0x3C8 });
                _coinsQuest.UpdateValue(_ram.ReadUint32(coinAddress));
            }
            catch
            { }
            
            return true;
        }
    }
}
