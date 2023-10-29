using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class PokemonPuzzleChallengeConnector : IGameboyColorConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            GoalValue = 2500,
            MaxIncrease = 500,
        };

        public PokemonPuzzleChallengeConnector()
        {
            Name = "Pokémon Puzzle Challenge";
            Description = "In a rising pit of colored blocks, it's your task to continuously eliminate these tiles by switching two tiles' positions on the horizontal row. When three or more tiles link up in a up/down or left/right fashion, they disappear. Gravity kicks in and settles the rest of the tiles into the bin ? this can cause a chain reaction to occur since other blocks of the same color can fall into place next to each other. It's up to you to continuously work the bin to wipe out tiles by making combination connections and chain reactions. That's the game in a nutshell, but there are several modes of play that'll keep your fingers busy. In the game's Challenge mode, for example, you're up against a computer opponent and you'll have to make combos and chain reactions to send garbage blocks to his unseen screen -- but keep in mind your opponent's doing the same to you.";
            SupportedVersions.Add("PAL (🇪🇺)");
            SupportedVersions.Add("NTSC-U (🇺🇸)");
            SupportedVersions.Add("NTSC-J (🇯🇵)");
            Author = "Dinopony";
            CoverFilename = "pokemon_puzzle_challenge.png";

            Quests.Add(_scoreQuest);

            ValidROMs.Add("BPNP01");
            ValidROMs.Add("BPNE01");
            ValidROMs.Add("BPNJ01");
        }

        protected override bool Poll()
        {
            // Score is placed slightly differently in the Japanese version
            long scoreOffset = (CurrentROM == "BPNJ01") ? 0x822 : 0x842;
            _scoreQuest.UpdateValue(_ram.ReadUint16(RamBaseAddress + scoreOffset));

            return true;
        }
    }
}
