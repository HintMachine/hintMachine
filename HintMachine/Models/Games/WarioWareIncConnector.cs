using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class WarioWareIncConnector : IGameboyAdvanceConnector
    {
        private readonly HintQuestCumulative _minigamesQuest = new HintQuestCumulative
        {
            Name = "Minigames count",
            Description = "Complete the task",
            GoalValue = 30,
            MaxIncrease = 2,
        };

        public WarioWareIncConnector()
        {
            Name = "WarioWare, Inc.: Mega Microgames!";
            Description = "Frantic action! Prepare for lightning-quick game play as you blaze through over 200 bizarre microgames designed by a crazy crew of Wario's cronies! " +
                "There are even two-player contests that can be played on a single Game Boy Advance! " +
                "Pick up and play! Ultra-simple controls make each game easy to get into...until the games start coming faster... and faster...and FASTER!";
            SupportedVersions.Add("EU ROM");
            Author = "CalDrac";
            CoverFilename = "wariowareinc.png";

            Quests.Add(_minigamesQuest);

            ValidROMs.Add("AZWP01");
        }

        protected override bool Poll()
        {
            _minigamesQuest.UpdateValue(_ram.ReadUint8(ExternalRamBaseAddress + 0x439EC));
            return true;
        }
    }
}
