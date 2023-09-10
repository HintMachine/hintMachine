using System;

namespace HintMachine.Games
{
    public class OneFingerDeathPunchConnector : IGameConnectorProcess32Bit
    {
        private long _previousKills = 0;

        private readonly HintQuest _killsQuest;

        public OneFingerDeathPunchConnector() : base("One Finger Death Punch")
        {
            _killsQuest = new HintQuest("Kills", 450);
            quests.Add(_killsQuest);
        }

        public override string GetDisplayName()
        {
            return "One Finger Death Punch";
        }

        public override void Poll()
        {
            if (process == null || module == null)
                return;

            uint baseAddress = ProcessUtils32.CheatengineSpecific.GetThreadStack0(process);
            long survivalKillsAddress = ResolvePointerPath(baseAddress, new int[] { -0x8c8, 0x644, 0x90 });

            long survivalKills = ReadInt64(survivalKillsAddress);
            if (survivalKills > _previousKills)
                _killsQuest.Add(survivalKills - _previousKills);
            _previousKills = survivalKills;
        }
    }
}
