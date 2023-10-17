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

        enum Region
        {
            PAL,
            NTSC_U,
            NTSC_J
        }
        private Region _region = Region.PAL;

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
        }

        public override bool Connect()
        {
            if (!base.Connect()) 
                return false;

            // Look for the PAL ROM first.
            byte[] MPP_SIG = new byte[] { 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0x05, 0xE9 };
            if (FindRamSignature(MPP_SIG, 0))
            {
                _region = Region.PAL;
                return true;
            }

            // If we didn't find the PAL ROM, then look for the NTSC-U ROM.
            MPP_SIG = new byte[] { 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0x43, 0xF3 };
            if (FindRamSignature(MPP_SIG, 0))
            {
                _region = Region.NTSC_U;
                return true;
            }

            // If we didn't find the NTSC-U ROM either, then look for the NTSC-J ROM.
            MPP_SIG = new byte[] { 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0xFF, 0xE7, 0xFF, 0xDE, 0x4D, 0x3D };
            if (FindRamSignature(MPP_SIG, 0))
            {
                _region = Region.NTSC_J;
                return true;
            }

            // If we didn't find any of them, then return false.
            return false;
        }

        public override bool Poll()
        {
            switch (_region)
            {
                case Region.PAL:
                    _pointsQuest.UpdateValue(_ram.ReadInt32(_dsRamBaseAddress + 0x3BB9B4));
                    _artifactsQuest.UpdateValue(_ram.ReadInt32(_dsRamBaseAddress + 0x3D428C));
                    break;
                case Region.NTSC_U:
                    _pointsQuest.UpdateValue(_ram.ReadInt32(_dsRamBaseAddress + 0x3AFC50));
                    _artifactsQuest.UpdateValue(_ram.ReadInt32(_dsRamBaseAddress + 0x3C7658));
                    break;
                case Region.NTSC_J:
                    _pointsQuest.UpdateValue(_ram.ReadInt32(_dsRamBaseAddress + 0x3B5C50));
                    _artifactsQuest.UpdateValue(_ram.ReadInt32(_dsRamBaseAddress + 0x3CDCD4));
                    break;
            }
            return true;
        }
    }
}
