using System;
using System.Collections.Generic;

namespace HintMachine.Games
{
    public class XenotiltConnector : IGameConnectorProcess32Bit
    {
        private long _previousScore = 0;

        private readonly HintQuest _scoreQuest;

        public XenotiltConnector() : base("Xenotilt", "mono-2.0-bdwgc.dll")
        {
            _scoreQuest = new HintQuest("Score", 200000000);
            quests.Add(_scoreQuest);
        }

        public override string GetDisplayName()
        {
            return "Xenotilt";
        }

        public override void Poll()
        {
            if (process == null || module == null)
                return;

            long baseAddress = module.BaseAddress.ToInt64() + 0x7270B8;
            long scoreAddress = ResolvePointerPath(baseAddress, new int[] { 0x30, 0x7e0, 0x7C0 });
            
            long score = ReadInt64(scoreAddress);
            if (score > _previousScore)
                _scoreQuest.Add(score - _previousScore);
            _previousScore = score;
        }
    }
}
