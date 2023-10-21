using System.Collections.Generic;

namespace HintMachine.GenericConnectors
{
    public abstract class IEmulatorConnector : IGameConnector
    {
        public List<string> ValidROMs { get; private set; } = new List<string>();

        public string CurrentROM { get; private set; } = "";

        private long _previousFrameCount = 0;

        private long _tickId = 0;

        // -------------------------------------
        public IEmulatorConnector()
        {}

        public override void Disconnect()
        {
            _previousFrameCount = 0;
            _tickId = 0;
        }

        protected override bool AfterConnect()
        {
            if (!TestRomIdentity())
            {
                Logger.Debug($"Found a valid emu process with invalid ROM identity '{CurrentROM}'");
                return false;
            }

            return true;
        }

        protected override bool BeforePoll()
        {
            // Every 10 ticks (~1s), check if the ROM identity has changed to ensure there was no ROM swapping.
            // If ROM was indeed changed, cut the connection.
            if (++_tickId % 10 == 0 && !TestRomIdentity())
            {
                Logger.Debug("ROM identity has become wrong, disconnecting...");
                return false;
            }

            // Every tick, check if frame count went backwards to detect save state usage. If any save state
            // was used, cut the connection.
            long currentFrameCount = GetCurrentFrameCount();
            if (currentFrameCount != 0 && currentFrameCount < _previousFrameCount)
            {
                Logger.Debug($"Frame count rollback detected ({_previousFrameCount} -> {currentFrameCount}), disconnecting...");
                return false;
            }
            _previousFrameCount = currentFrameCount;

            return true;
        }

        public bool TestRomIdentity()
        {
            CurrentROM = GetRomIdentity();
            if (CurrentROM != "" && ValidROMs.Count > 0 && !ValidROMs.Contains(CurrentROM))
                return false;
            return true;
        }

        public abstract long GetCurrentFrameCount();

        public abstract string GetRomIdentity();
    }
}
