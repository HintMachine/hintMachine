using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    class MetroidPrimePinballConnector : INintendoDSConnector
    {
        private readonly HintQuestCumulative _pointsQuest = new HintQuestCumulative
        {
            Name = "Points",
            GoalValue = 500000
        };
        private readonly HintQuestCumulative _artifactsQuest = new HintQuestCumulative
        {
            Name = "Chozo Artifacts",
            Description = "Only applicable in Multi-Mission mode.",
            GoalValue = 3
        };

        private const string REGION_PAL = "93B08A557E3B630FB25F9F331FF479974B21C017D16DA026DDA270504C62106C";
        private const string REGION_NTSC_U = "C724C1CE61F8E34393337C626EF462CB5E5BD84F3EB5A70985946127FDB8EFA1";
        private const string REGION_NTSC_J = "133184DE3C5AD82713347F41B83E8C354DACFE0C6F3FB4C20C57765D482CBF50";

        // ---------------------------------------------------------

        public MetroidPrimePinballConnector()
        {
            Name = "Metroid Prime Pinball";
            Description = "Metroid Prime, but abridged as a Pinball game.\n\nSamus Aran's entry into the bumper-and-flipper world is a sleek, sci-fi classic gaming adventure that has her careening into gigantic boss monsters and bouncing through a variety of exciting pinball tables. Play tables across two screens at the same time using the touch screen to nudge the pinball table. Battle deadly enemies and experience a number of special modes such as Clone Machine Multiball and the Wall-Jump Challenge while you blast and bomb your high score into a state of pure pinball pandemonium.";
            SupportedVersions.Add("PAL (🇪🇺)");
            SupportedVersions.Add("NTSC-U (🇺🇸)");
            SupportedVersions.Add("NTSC-J (🇯🇵)");
            CoverFilename = "metroid_prime_pinball.png";
            Author = "Knuxfan24";

            Quests.Add(_pointsQuest);
            Quests.Add(_artifactsQuest);

            ValidROMs.Add(REGION_PAL);
            ValidROMs.Add(REGION_NTSC_U);
            ValidROMs.Add(REGION_NTSC_J);
        }

        public override bool Poll()
        {
            if(!base.Poll())
                return false;

            if (CurrentROM == REGION_PAL)
            {
                _pointsQuest.UpdateValue(_ram.ReadInt32(RamBaseAddress + 0x3BB9B4));
                _artifactsQuest.UpdateValue(_ram.ReadInt32(RamBaseAddress + 0x3D428C));
            }
            else if (CurrentROM == REGION_NTSC_U)
            {
                _pointsQuest.UpdateValue(_ram.ReadInt32(RamBaseAddress + 0x3AFC50));
                _artifactsQuest.UpdateValue(_ram.ReadInt32(RamBaseAddress + 0x3C7658));
            }
            else if (CurrentROM == REGION_NTSC_J)
            {
                _pointsQuest.UpdateValue(_ram.ReadInt32(RamBaseAddress + 0x3B5C50));
                _artifactsQuest.UpdateValue(_ram.ReadInt32(RamBaseAddress + 0x3CDCD4));
            }

            return true;
        }
    }
}
