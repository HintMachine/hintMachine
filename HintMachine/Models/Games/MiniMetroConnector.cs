using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class MiniMetroConnector : IGameConnector
    {
        private readonly HintQuestCumulative _passengersQuest = new HintQuestCumulative
        {
            Name = "Passengers Delivered",
            GoalValue = 150,
            MaxIncrease = 10,
        };

        private ProcessRamWatcher _ram = null;
        private uint? _lastValidValue = null;

        public MiniMetroConnector()
        {
            Name = "Mini Metro";
            Description = "Mini Metro is a strategy simulation game about designing a subway map for a growing city.\n\nDraw lines between stations and start your trains running. As new stations open, redraw your lines to keep them efficient. Decide where to use your limited resources. How long can you keep the city moving?";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "mini_metro.png";
            Author = "Chandler";

            Quests.Add(_passengersQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("MiniMetro", "mono-2.0-bdwgc.dll");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            try
            {
                long address = _ram.ResolvePointerPath32(_ram.BaseAddress + 0x0058010C, new int[] { 0x5EC, 0x24, 0x8, 0x54, 0x10 });
                uint value = _ram.ReadUint32(address);
                if (address == 0)
                    return true;

                if (_lastValidValue.HasValue)
                {
                    long delta = (long)value - (long)_lastValidValue.Value;

                    // Returning to Menu causes Passenger Count to become -1 (255 unsigned int), which is frequently flagging the MaxIncrease warning.
                    // This hides this warning when the value was specifically set to 255 from far away.
                    bool likelyMainMenuReturn = (value == 255 && delta > _passengersQuest.MaxIncrease);
                    if (likelyMainMenuReturn)
                        return true;
                }

                _passengersQuest.UpdateValue(value);
                _lastValidValue = value;
            }
            catch (ProcessRamWatcherException)
            {
                if (_ram?.Process == null || _ram.Process.HasExited)
                    return false;

                return true;
            }

            return true;
        }
    }
}
