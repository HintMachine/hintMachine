using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class PapersPleaseConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "PapersPlease",
            ModuleName = "GameAssembly.dll",
            Hash = "9451692A33540D96352C290E1FB5FDADE29B2B112206CCD40D711E271D2886B7"
        };

        private long _previousPoints = 0;

        private readonly HintQuestCounter _entrantsQuest = new HintQuestCumulative
        {
            Name = "Entrants Correctly Processed",
            GoalValue = 5,
            MaxIncrease = 1,
            Description = "Endless Mode only"
        };

        private ProcessRamWatcher _ram = null;

        public PapersPleaseConnector()
        {
            Name = "Papers, Please";
            Description = "Congratulations. The October labor lottery is complete. Your name was pulled. For immediate placement, report to the Ministry of Admission at Grestin Border Checkpoint. An apartment will be provided for you and your family in East Grestin. Expect a Class-8 dwelling.\n\nYour job as immigration inspector is to control the flow of people entering the Arstotzkan side of Grestin from Kolechia. Among the throngs of immigrants and visitors looking for work are hidden smugglers, spies, and terrorists.\n\nUsing only the documents provided by travelers and the Ministry of Admission's primitive inspect, search, and fingerprint systems you must decide who can enter Arstotzka and who will be turned away or arrested.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "papers_please.png";
            Author = "Chandler";

            Quests.Add(_entrantsQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(GAME_VERSION_STEAM);
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            if (!_ram.TestProcess())
                return false;

            try {
                long address = _ram.ResolvePointerPath64(_ram.BaseAddress + 0xC6B6A0, new int[] { 0xB8, 0x10, 0x50, 0x18, 0x28, 0xB8, 0x38 });
                long points = _ram.ReadUint32(address);

                if (points > _previousPoints && points - _previousPoints <= 10) {
                    _entrantsQuest.CurrentValue += 1;
                }

                _previousPoints = points;
            }
            catch
            { }
            
            return true;
        }
    }
}
