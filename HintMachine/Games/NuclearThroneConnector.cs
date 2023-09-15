using System;
using System.Collections.Generic;

namespace HintMachine.Games
{
    public class NuclearThroneConnector : IGameConnectorProcess
    {
        private uint _previousScore = uint.MaxValue;
        private double _previousLevel = 0;
        private bool rewardedLvMax = false;
        private readonly HintQuest _levelQuest = new HintQuest("Reach level max", 10, "objective", 2);

        public NuclearThroneConnector() : base("nuclearthrone")
        {
            quests.Add(_levelQuest);
        }

        public override string GetDisplayName()
        {
            return "Nuclear Throne";
        }

        public override string GetDescription()
        {
            return "Kill your way to the Nuclear Throne" +
                "Tested on up-to-date Steam version.";
        }

        public override bool Poll()
        {
            if (process == null || module == null)
                return false;

            long baseAddress = module.BaseAddress.ToInt32() + 0x074E3ED8;
            long levelAddress = ResolvePointerPath32(baseAddress, new int[] { 0x58, 0xC, 0x5D8, 0x530 });

            double level = ReadDouble(levelAddress);

            long res;
            if (long.TryParse(level.ToString(), out res))
                _levelQuest.SetValue(res);
            else
                _levelQuest.SetValue(0);

            if (level < _previousLevel) {
                _levelQuest.hasBeenAwarded = false;
            }
            _previousLevel = level;

            return true;
        }
    }
}