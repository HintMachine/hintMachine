using System;
using System.Collections.Generic;

namespace HintMachine.Games
{
    public class NuclearThroneConnector : IGameConnectorProcess
    {
        private double _previousHealth = 0;
        private double _previousLevel = 0;
        private bool rewardedLvMax = false;
        private readonly HintQuest _levelQuest = new HintQuest("Reach level max", 10, "objective", 2);
        private readonly HintQuest _killThroneQuest = new HintQuest("Kill Nuclear Throne", 1, "objective", 2);

        public NuclearThroneConnector() : base("NuclearThroneTogether")
        {
            //quests.Add(_levelQuest);
            quests.Add(_killThroneQuest);
        }

        public override string GetDisplayName()
        {
            return "Nuclear Throne";
        }

        public override string GetDescription()
        {
            return "Kill your way to the Nuclear Throne \n" +
                "NuclearThroneTogether mod is needed \n";
        }

        public override bool Poll()
        {
            if (process == null || module == null)
                return false;

            //long baseAddress = module.BaseAddress.ToInt32() + 0x074E3ED8;
            //long levelAddress = ResolvePointerPath32(baseAddress, new int[] { 0x58, 0xC, 0x5D8, 0x530 });

            //double level = ReadDouble(levelAddress);
            double level = 0;
            long res;
            if (long.TryParse(level.ToString(), out res))
                _levelQuest.SetValue(res);
            else
                _levelQuest.SetValue(0);

            if (level < _previousLevel)
            {
                _levelQuest.hasBeenAwarded = false;
            }
            _previousLevel = level;

            //Praying addresses in NuclearThroneTogether and nonmodded are the same
            long baseThroneAdress = module.BaseAddress.ToInt32() + 0x081D2A24;

                long throneLiveAddress = ResolvePointerPath32(baseThroneAdress, new int[] { 0x98, 0xC4, 0x8, 0x44, 0x10, 0x2d4, 0x0 });
                //Check if throne has awoken
                if (throneLiveAddress != 0)
                {
                    double currentThroneHealth = ReadDouble(throneLiveAddress);
                Console.WriteLine("Health :" + currentThroneHealth);
                    if (currentThroneHealth <= 0)
                    {
                        _killThroneQuest.Add(1);
                    }
                    _previousHealth = currentThroneHealth;
                }
                else {
                    if (_previousHealth != 0 && _previousHealth > 0)
                    {
                        _killThroneQuest.Add(1);
                        _previousHealth = 0;
                    }
                }

            return true;
        }
    }
}