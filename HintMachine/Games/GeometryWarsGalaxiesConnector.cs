using HintMachine.GenericConnectors;

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
            Name = "Geometry Wars: Galaxies";
            Description = "Geometry Wars: Galaxies is set in a space-like environment where the player must shoot geometrical shapes in order to score points, gain lives, acquire bombs and survive as long as possible. The game is played to an upbeat electro soundtrack.";
            SupportedVersions.Add("Europe");
            CoverFilename = "geometry_wars_galaxies.png";
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
