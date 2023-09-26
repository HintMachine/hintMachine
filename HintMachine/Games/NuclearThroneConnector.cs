using System;

namespace HintMachine.Games
{
    public class NuclearThroneConnector : IGameConnector
    {
        private readonly HintQuestCounter _killThroneQuest = new HintQuestCounter
        {
            Name = "Wins",
            Description = "Sit on the Nuclear Throne and wait for the credits to end",
            AwardedHints = 2
        };

        private ProcessRamWatcher _ram = null;
        private IntPtr _threadstack0Addr;
        private uint _previousWinValue = 0;

        public NuclearThroneConnector()
        {
            Name = "Nuclear Throne";
            Description = "Kill your way to the Nuclear Throne\n\n" +
                          "NuclearThroneTogether mod is required.";
            Quests.Add(_killThroneQuest);
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

        public override bool Poll()
        {
            syncThreadStackAdr();

            int[] OFFSETS = new int[] { 0xA0, 0x60C, 0x104, 0x714, 0x0 };
            try
            {
                long throneAliveAddress = _ram.ResolvePointerPath32(_threadstack0Addr.ToInt32() - 0x638, OFFSETS);
                if (throneAliveAddress == 0)
                    return true;
                
                uint winValue = _ram.ReadUint32(throneAliveAddress);
                if (winValue == 542461785 && _previousWinValue != winValue)
                    _killThroneQuest.CurrentValue += 1;
                _previousWinValue = winValue;
            }
            catch {}
            return true;
            
        }

        private async void syncThreadStackAdr()
        {
            _threadstack0Addr = (IntPtr)await _ram.GetThreadstack0Address();
        }
    }
}