using System;
using System.Collections.Generic;

namespace HintMachine.Games
{
    public class GeometryWarsConnector : IGameConnectorProcess
    {
        private uint _previousScore = uint.MaxValue;
        private readonly HintQuest _scoreQuest = new HintQuest("Score", 20000);

        public GeometryWarsConnector() : base("GeometryWars")
        {
            quests.Add(_scoreQuest);
        }

        public override string GetDisplayName()
        {
            return "Geometry Wars: Retro Evolved";
        }

        public override string GetDescription()
        {
            return "Kill geometric ennemies in this box shaped area twin stick shooter." +
                "Tested on up-to-date Steam version.";
        }

        public override bool Poll()
        {
            if (process == null || module == null)
                return false;

            long baseAddress = module.BaseAddress.ToInt64() + 0x00170084;
            long scoreAddress = ResolvePointerPath32(baseAddress, new int[] { 0x40 });

            uint score = ReadUint32(scoreAddress);
            if (score > _previousScore)
                _scoreQuest.Add(score - _previousScore);
            _previousScore = score;

            return true;
        }
    }
}