using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    class PokemonPinballRSConnector : IGameConnector
    {
        private readonly HintQuestCumulative _pokemonQuest = new HintQuestCumulative
        {
            Name = "Pokémon count",
            Description = "Catch, hatch and evolve Pokemon",
            GoalValue = 3,
            MaxIncrease = 1,
        };

        protected ProcessRamWatcher _ram = null;

        public PokemonPinballRSConnector()
        {
            Name = "Pokémon Pinball: Ruby & Sapphire";
            Description = "Pokémon Pinball has all the features you'd demand of a pinball game, including bonus tables, lots of bumpers and ways to score massive points." +
                "Instead of a ball, you make use of a Pokéball. Instead of standard bumpers, you're hitting the Pokéball against other Pokémon, and the ultimate goal is of course to \"catch 'em all\". The game features 200 Pokémon and two main tables.";
            SupportedVersions.Add("EU ROM");
            Author = "CalDrac";
            Platform = "Gameboy Advance";
            CoverFilename = "pokemon_pinball_rs.png";
            Quests.Add(_pokemonQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("EmuHawk", "mgba.dll");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long pokemonAddr = _ram.ResolvePointerPath64( _ram.BaseAddress + 0x00103448, new int[] { 0x10, 0x28, 0x5F0 });
            _pokemonQuest.UpdateValue(_ram.ReadUint8(pokemonAddr));
            return true;
        }
    }
}
