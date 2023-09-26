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
        private IntPtr Thread0Address;
        private uint _previousWinValue = 0;

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
            return "Kill your way to the Nuclear Throne\n\n" +
                   "NuclearThroneTogether mod is required.\n";
        }

        public override bool Poll()
        {
            syncThreadStackAdr();

            int[] OFFSETS = new int[] { 0xA0, 0x60C, 0x104, 0x714, 0x0 };
            try
            {
                long throneAliveAddress = _ram.ResolvePointerPath32(Thread0Address.ToInt32() - 0x638, OFFSETS);
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