using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    public class GeometryWarsGalaxiesConnector : IDolphinConnector
    {
        private readonly HintQuestCumulative _geomsQuest = new HintQuestCumulative
        {
            Name = "Geoms collected",
            GoalValue = 10000,
            MaxIncrease = 200,
        };
      
        // ------------------------------------------------------

        public GeometryWarsGalaxiesConnector() : base(true)
        {
            Name = "Geometry Wars: Galaxies";
            Description = "Geometry Wars: Galaxies is set in a space-like environment where the player must shoot geometrical shapes in order to score points, gain lives, acquire bombs and survive as long as possible. The game is played to an upbeat electro soundtrack.";
            SupportedVersions.Add("Europe");
            CoverFilename = "geometry_wars_galaxies.png";
            Author = "Dinopony";

            Quests.Add(_geomsQuest);

            ValidROMs.Add("RGLP7D"); // PAL
        }

        protected override bool Poll()
        {
            uint geoms = _ram.ReadUint32(MEM2 + 0x23A2AF4);
            _geomsQuest.UpdateValue(geoms);

            return true;
        }
    }
}
