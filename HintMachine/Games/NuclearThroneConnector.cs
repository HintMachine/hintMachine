using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HintMachine.Games
{
    public class NuclearThroneConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private double _previousHealth = 0;
        private double _previousLevel = 0;
        private bool rewardedLvMax = false;
        private readonly HintQuest _killThroneQuest = new HintQuest("Sit on the Nuclear Throne \n and wait for the credits end", 1, "objective", 2);
        private IntPtr Thread0Address;
        public NuclearThroneConnector()
        {
            quests.Add(_killThroneQuest);
        }

        public override string GetDisplayName()
        {
            return "Nuclear Throne";
        }

        public override string GetDescription()
        {
            return "Kill your way to the Nuclear Throne \n" +
                "NuclearThroneTogether mod is required \n";
        }

        public override bool Poll()
        {
            syncThreadStackAdr();

            int[] OFFSETS = new int[] { 0xA0, 0x60C, 0x104, 0x714, 0x0 };
            try
            {
                long throneAliveAddress = _ram.ResolvePointerPath32(Thread0Address.ToInt32() - 0x638, OFFSETS);
                if (throneAliveAddress != 0)
                {
                    uint winValue = _ram.ReadUint32(throneAliveAddress);
                    if (winValue == 542461785)
                    {
                        if (!_killThroneQuest.hasBeenAwarded)
                        {
                            _killThroneQuest.Add(1);
                        }
                    }
                    else
                    {
                        _killThroneQuest.SetValue(0);
                    }
                }
            }
            catch {}
            return true;
            
        }


        private async void syncThreadStackAdr()
        {
            Thread0Address = (IntPtr)await _ram.getThread0Address();
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("nuclearthronetogether");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }
    }
}