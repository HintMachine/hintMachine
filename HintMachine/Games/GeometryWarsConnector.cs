using System;
using System.Collections.Generic;

namespace HintMachine.Games
{
    public class GeometryWarsConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private uint _previousScore = uint.MaxValue;
        private readonly HintQuest _scoreQuest = new HintQuest("Score", 20000);

        public GeometryWarsConnector()
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


        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("GeometryWars");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long scoreAddress = _ram.ResolvePointerPath32(_ram.baseAddress + 0x170084, new int[] { 0x40 });

            uint score = _ram.ReadUint32(scoreAddress);
            if (score > _previousScore)
                _scoreQuest.Add(score - _previousScore);
            _previousScore = score;

            return true;
        }
    }
}