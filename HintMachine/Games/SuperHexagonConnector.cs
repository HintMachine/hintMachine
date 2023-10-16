namespace HintMachine.Games
{
    public class SuperHexagonConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "superhexagon",
            Hash = "72B0C26053C37EDD3435DEF461E9027CD6FFAD12032DB2FD0B32C256FDBEE6B9"
        };

        private readonly BinaryTarget GAME_VERSION_ITCH = new BinaryTarget
        {
            DisplayName = "itch",
            ProcessName = "superhexagon",
            Hash = "A68BFFBDBC6C1C6C625D6E9F565C6DFDC1D517A98F781BDBD5D7C96636538625"
        };

        private readonly HintQuestCumulative _timeQuest = new HintQuestCumulative
        {
            Name = "Time Survived",
            Description = "Survive for 2 minutes. Time only counts if you survive for more than 10 seconds in a round.",
            GoalValue = 120,
            MaxIncrease = 15,
        };

        private ProcessRamWatcher _ram = null;

        private bool _past10seconds = false;

        // ------------------------------------------------------------

        public SuperHexagonConnector()
        {
            Name = "Super Hexagon";
            Description = "Super Hexagon is a minimal action game by Terry Cavanagh, with music by Chipzel.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            SupportedVersions.Add("itch.io");
            CoverFilename = "super_hexagon.png";
            Author = "Chandler (itch.io), Dinopony (Steam)";

            Quests.Add(_timeQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher();
            _ram.SupportedTargets.Add(GAME_VERSION_STEAM);
            _ram.SupportedTargets.Add(GAME_VERSION_ITCH);
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            if (!_ram.TestProcess())
                return false;

            long timeReading = 0;

            if(_ram.CurrentTarget == GAME_VERSION_STEAM)
            {
                long timeAddress = _ram.ResolvePointerPath32(_ram.BaseAddress + 0x15E8EC, new int[] { 0x2928 });
                if(timeAddress != 0)
                    timeReading = (long)(_ram.ReadDouble(timeAddress)) / 60;
            }
            else if(_ram.CurrentTarget == GAME_VERSION_ITCH)
            {
                long timeAddress = _ram.ResolvePointerPath32(_ram.BaseAddress + 0x29EB94, new int[] { 0xC, 0x10 });
                if(timeAddress != 0)
                    timeReading = _ram.ReadUint32(timeAddress + 0x5518) / 60;
            }

            if (timeReading > 0)
            {
                if (_past10seconds)
                {
                    if (timeReading < 10)
                    {
                        _past10seconds = false;
                    }
                    else
                    {
                        _timeQuest.UpdateValue(timeReading);
                    }
                }
                else if (timeReading >= 10)
                {
                    _past10seconds = true;
                    _timeQuest.UpdateValue(0);
                }
            }
            
            return true;
        }
    }
}