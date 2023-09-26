using System;

namespace HintMachine.Games
{
    public class OneFingerDeathPunchConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;

        private readonly HintQuestCumulative _killsQuest = new HintQuestCumulative
        {
            Name = "Kills",
            GoalValue = 450,
            Description = "Kill enemies in story mode or survival to progress"
        };

        private IntPtr _threadstack0Address;

        public OneFingerDeathPunchConnector()
        {
            quests.Add(_killsQuest);
        }

        public override string GetDisplayName()
        {
            return "One Finger Death Punch";
        }
        
        public override string GetDescription()
        {
            return "Fight using only your left and right mouse button. Your cursor can be anywhere on the screen.\n\n" +
                   "Tested on up-to-date Steam version";
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
            _threadstack0Address = (IntPtr)await _ram.getThread0Address();
        }

    }
}
