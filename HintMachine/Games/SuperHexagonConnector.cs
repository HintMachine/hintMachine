using System;

namespace HintMachine.Games
{
    public class SuperHexagonConnector : IGameConnector
    {
        private readonly HintQuestCumulative _timeQuest = new HintQuestCumulative
        {
            Name = "Time Survived",
            Description = "Survive for 2 minutes. Time only counts if you survive for more than 10 seconds in a round.",
            GoalValue = 7200,
            MaxIncrease = 700,
        };

        private ProcessRamWatcher _ram = null;

        private bool _past10seconds = false;

        public SuperHexagonConnector()
        {
            Name = "Super Hexagon";
            Description = "Super Hexagon is a minimal action game by Terry Cavanagh, with music by Chipzel.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            SupportedVersions.Add("itch.io");
            CoverFilename = "super_hexagon.png";
            Author = "Chandler";

            Quests.Add(_timeQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("superhexagon");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            try {
                long timeAddress = _ram.ResolvePointerPath32(_ram.BaseAddress + 0x29C0D4, new int[] { 0x20, 0x0, 0x18, 0x0, 0xB4, 0x18 });
                long timeReading = _ram.ReadUint32(timeAddress + 0x5518);
                if (_past10seconds) {
                    if (timeReading < 600) {
                        _past10seconds = false;
                    } else {
                        _timeQuest.UpdateValue(timeReading);
                    }
                } else if (timeReading >= 600) {
                    _past10seconds = true;
                    _timeQuest.UpdateValue(0);
                }
            }
            catch
            { }
            
            return true;
        }
    }
}