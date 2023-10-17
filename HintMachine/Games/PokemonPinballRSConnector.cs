using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    class PokemonPinballRSConnector : IGameConnector //GBA connector can be implemented by skilled people
    {
        private readonly HintQuestCumulative _pokemonQuest = new HintQuestCumulative
        {
            Name = "Pokemon count",
            Description = "Catch, hatch and evolve Pokemon",
            GoalValue = 3,
            MaxIncrease = 1,
        };

        protected ProcessRamWatcher _ram = null;

        private long _pokemonAddr = 0;
        public PokemonPinballRSConnector()
        {
            Name = "Pokemon Pinball: Ruby & Sapphire";
            Description = "Pokemon Pinball has all the features you'd demand of a pinball game, including bonus tables, lots of bumpers and ways to score massive points." +
                "Instead of a ball, you make use of a Pokeball. Instead of standard bumpers, you're hitting the Pokeball against other Pokemon, and the ultimate goal is of course to \"catch 'em all\". The game features 200 Pokemon and two main tables.";
            SupportedVersions.Add("EU ROM");
            Author = "CalDrac";
            Platform = "Gameboy Advance";
            CoverFilename = "pkmnPinballRS.png";
            Quests.Add(_pokemonQuest);
        }

        public override bool Connect()
        {

            _ram = new ProcessRamWatcher("EmuHawk", "mgba.dll");
            if (!_ram.TryConnect())
                return false;

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null; ;
        }

        public override bool Poll()
        {
            long _pokemonAddr = _ram.ResolvePointerPath64( _ram.BaseAddress + 0x00103448, new int[] { 0x10, 0x28, 0x5F0 });
            _pokemonQuest.UpdateValue(_ram.ReadUint8(_pokemonAddr));
            return true;
        }
    }
}
