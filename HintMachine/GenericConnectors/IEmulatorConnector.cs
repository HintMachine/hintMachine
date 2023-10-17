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

        public override bool Poll()
        {
            // Every 10 ticks (~1s), check if the ROM identity has changed to ensure there was no ROM swapping.
            // If ROM was indeed changed, cut the connection.
            if (++_tickId % 10 == 0 && !TestRomIdentity())
                return false;

            // Every tick, check if frame count went backwards to detect save state usage. If any save state
            // was used, cut the connection.
            long currentFrameCount = GetCurrentFrameCount();
            if (currentFrameCount < _previousFrameCount)
                return false;
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
