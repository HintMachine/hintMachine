using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class PokemonPinballRSConnector : IGameboyAdvanceConnector
    {
        private readonly HintQuestCumulative _pokemonQuest = new HintQuestCumulative
        {
            Name = "Pokémon count",
            Description = "Catch, hatch and evolve Pokémon",
            GoalValue = 3,
            MaxIncrease = 1,
        };

        public PokemonPinballRSConnector()
        {
            Name = "Pokémon Pinball: Ruby & Sapphire";
            Description = "Pokémon Pinball has all the features you'd demand of a pinball game, including bonus tables, lots of bumpers and ways to score massive points." +
                "Instead of a ball, you make use of a Pokéball. Instead of standard bumpers, you're hitting the Pokéball against other Pokémon, and the ultimate goal is of course to \"catch 'em all\". The game features 200 Pokémon and two main tables.";
            SupportedVersions.Add("EU ROM");
            Author = "CalDrac";
            CoverFilename = "pokemon_pinball_rs.png";

            Quests.Add(_pokemonQuest);

            ValidROMs.Add("BPPP01");
        }

        protected override bool Poll()
        {
            _pokemonQuest.UpdateValue(_ram.ReadUint8(ExternalRamBaseAddress + 0x5F0));
            return true;
        }
    }
}
