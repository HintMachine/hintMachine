using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class BurnoutRevengeConnector : IGameConnector
    {
        private readonly HintQuestCumulative _trafficCarsQuest = new HintQuestCumulative
        {
            Name = "Hit traffic cars",
            GoalValue = 100,
            MaxIncrease = 10,
        };

        private readonly HintQuestCumulative _takeDownQuest = new HintQuestCumulative
        {
            Name = "Takedown",
            GoalValue = 50,
            MaxIncrease = 3,
        };

        private ProcessRamWatcher _ram = null;
        private long _trafficAddr = 0;
        private long _takedownAddr = 0;

        public BurnoutRevengeConnector()
        {
            Name = "Burnout Revenge";
            Description = "In Burnout Revenge, players compete in a range of racing game types with different aims. These take place within rush-hour traffic, and include circuit racing, Road Rage (where players cause as many rivals to crash as possible within a time limit, or until the player's car is wrecked), Burning Lap (a single-lap, single-racer time attack mode), Eliminator (a circuit race where every thirty seconds, the last-placed racer's car is detonated; the race continues until only one racer is left), and Crash (where the player is placed at a junction with the aim of accumulating as many \"Crash Dollars\" as possible";
            SupportedVersions.Add("US ISO");
            SupportedEmulators.Add("PCSX2 Nightly v1.7.5158");

            CoverFilename = "burnoutRevenge.png";
            Platform = "PS2";
            Author = "CalDrac";

            Quests.Add(_trafficCarsQuest);
            Quests.Add(_takeDownQuest);
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("pcsx2-qt");
            return _ram.TryConnect();
        }

        protected override bool Poll()
        {
            _trafficAddr = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x030B98B8, new int[] { 0x1B0 });
            _trafficCarsQuest.UpdateValue(_ram.ReadInt32(_trafficAddr));
            _takedownAddr = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x030B8860, new int[] { 0x49C });
            _takeDownQuest.UpdateValue(_ram.ReadInt32(_takedownAddr));
            Console.WriteLine("Takedown : " + _ram.ReadInt32(_takedownAddr));
            return true;
        }
    }
}
