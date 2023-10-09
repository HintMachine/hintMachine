using System;

namespace HintMachine.Games
{
    public class OneFingerDeathPunchConnector : IGameConnector
    {
        private readonly HintQuestCumulative _killsQuest = new HintQuestCumulative
        {
            Name = "Kills",
            GoalValue = 450,
            Description = "Kill enemies in story mode or survival to progress",
            MaxIncrease = 100,
            Direction = HintQuestCumulative.CumulativeDirection.ASCENDING
            
        };

        private ProcessRamWatcher _ram = null;
        private IntPtr _threadstack0Address;

        public OneFingerDeathPunchConnector()
        {
            Name = "One Finger Death Punch";
            Description = "Fight using only your left and right mouse button. Your cursor can be anywhere on the screen.";
            SupportedVersions = "Tested on up-to-date Steam version";

            Quests.Add(_killsQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("One Finger Death Punch");
            if (!_ram.TryConnect())
                return false;

            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            syncThreadStackAdr();

            long killsAddress = _ram.ResolvePointerPath32(_threadstack0Address.ToInt32() - 0x8cc, new int[] { 0x644, 0x90 });
            _killsQuest.UpdateValue(_ram.ReadUint32(killsAddress));

            return true;
        }

        private async void syncThreadStackAdr()
        {
            _threadstack0Address = (IntPtr)await _ram.GetThreadstack0Address();
        }

    }
}
