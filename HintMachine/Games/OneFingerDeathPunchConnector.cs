namespace HintMachine.Games
{
    public class OneFingerDeathPunchConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private long _previousKills = long.MaxValue;
        private readonly HintQuest _killsQuest = new HintQuest("Kills", 450);

        public OneFingerDeathPunchConnector()
        {
            quests.Add(_killsQuest);
        }

        public override string GetDisplayName()
        {
            return "One Finger Death Punch";
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("One Finger Death Punch");
            if (!_ram.TryConnect())
                return false;

            _ram.UpdateThreadstack0();
            return (_ram.threadstack0 != 0);
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long killsAddress = _ram.ResolvePointerPath32(_ram.threadstack0 - 0x8cc, new int[] { 0x644, 0x90 });
            
            uint kills = _ram.ReadUint32(killsAddress);
            if (kills > _previousKills)
                _killsQuest.Add(kills - _previousKills);
            _previousKills = kills;

            return true;
        }
    }
}
