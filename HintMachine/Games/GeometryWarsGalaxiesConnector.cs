using System.Collections.Generic;
using System.Linq;
using static HintMachine.ProcessRamWatcher;

namespace HintMachine.Games
{
    public class GeometryWarsGalaxiesConnector : IDolphinConnector
    {
        private readonly HintQuestCumulative _geomsQuest = new HintQuestCumulative
        {
            Name = "Geoms collected",
            GoalValue = 10000
        };
      
        // ------------------------------------------------------

        public GeometryWarsGalaxiesConnector() : base(true, "RGLP7D")
        {
            Name = "Geometry Wars Galaxies (Wii)";
            Description = "Destroy geometric enemies in this classic and stylish twin-stick shooter " +
                          "in order to earn geoms and unlock hints.";
            SupportedVersions = "Tested on European ROM on all recent versions of Dolphin 5 (64-bit).";
            Author = "Dinopony";

            Quests.Add(_geomsQuest);
        }

        public override bool Poll()
        {
            long geomsAddr = _mem2Addr + 0x23A2AF4;
            _geomsQuest.UpdateValue(_ram.ReadUint32(geomsAddr, true));

            return true;
        }
    }
}
