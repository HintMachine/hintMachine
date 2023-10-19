using HintMachine.GenericConnectors;

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
        private uint _previousWinValue = 0;

        public NuclearThroneConnector()
        {
            Name = "Nuclear Throne";
            Description = "Kill your way to the Nuclear Throne";
            Platform = "PC";
            SupportedVersions.Add("NuclearThroneTogether mod");
            Author = "CalDrac";

            Quests.Add(_killThroneQuest);
        }
        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("nuclearthronetogether");
            _ram.Is64Bit = false;

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            int[] OFFSETS = new int[] { 0xA0, 0x60C, 0x104, 0x714, 0x0 };
            try
            {
                long throneAliveAddress = _ram.ResolvePointerPath32(_ram.Threadstack0 - 0x638, OFFSETS);
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
    }
}