using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    class Iridion2Connector : IGameboyAdvanceConnector
    {
        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            Description = "Catch, hatch and evolve Pokémon",
            GoalValue = 25000,
            MaxIncrease = 1000,
        };

        public Iridion2Connector()
        {
            Name = "Iridion II";
            Description = "Be the new saviour as the Iridion force strikes back! " +
                "A century has passed since the Iridion Empire was defeated. Humans have populated the old Iridion galaxy giving mankind freedom and peace. " +
                "Suddenly, all communications with the human outposts are lost." +
                " The Iridions are back! Planet Earth is too far away to offer help. " +
                "The only chance? A lone powerful spaceship...Yours!";

            SupportedVersions.Add("EU ROM");
            Author = "CalDrac";
            CoverFilename = "iridion2.png";

            Quests.Add(_scoreQuest);

            //ValidROMs.Add("BPPP01");
        }

        protected override bool Poll()
        {
            _scoreQuest.UpdateValue(_ram.ReadInt16(ExternalRamBaseAddress + 0x409d8));
            return true;
        }
    }
}
