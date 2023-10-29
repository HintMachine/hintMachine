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
            Description = "";
            SupportedVersions.Add("US ISO");
            SupportedEmulators.Add("PCSX2 Nightly v1.7.5158");

            //CoverFilename = "burnout_revenge.png";
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
