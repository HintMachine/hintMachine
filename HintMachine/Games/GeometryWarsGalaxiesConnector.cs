using System;
using System.Collections.Generic;

namespace HintMachine.Games
{
    public class GeometryWarsGalaxiesConnector : IGameConnectorProcess
    {
        private uint _previousGeoms = uint.MaxValue;
        private readonly HintQuest _geomsQuest = new HintQuest("Geoms collected", 1000);

        public GeometryWarsGalaxiesConnector() : base("Dolphin")
        {
            quests.Add(_geomsQuest);
        }

        public override string GetDisplayName()
        {
            return "Geometry Wars Galaxies";
        }

        public override void Poll()
        {
            if (process == null || module == null)
                return;

            long baseAddress = module.BaseAddress.ToInt64() + 0x1189050;
            long geomsAddress = ResolvePointerPath(baseAddress, new int[] { 0x0, 0x43A2AF6 });

            Console.WriteLine("geomsAddress = " + geomsAddress);

            uint geoms = ReadUint16(geomsAddress, true);
            if (geoms > _previousGeoms)
                _geomsQuest.Add(geoms - _previousGeoms);
            _previousGeoms = geoms;
        }
    }
}
