namespace HintMachine.Games
{
    public class SonicBlueSpheresConnector : IMegadriveConnector
    {
        private readonly HintQuestCounter _levelsQuest = new HintQuestCounter
        {
            Name = "Completed Levels",
            GoalValue = 2,
            TimeoutBetweenIncrements = 60,
        };
        private readonly HintQuestCumulative _ringsQuest = new HintQuestCumulative
        {
            Name = "Collected Rings",
            GoalValue = 200,
        };

        private ushort _previousBlueSpheresWinAnimProgress = 0;

        public SonicBlueSpheresConnector() : base()
        {
            Name = "Sonic 3 Blue Spheres";
            Description = "When you plugged the Sonic The Hedgehog cartridge into the Sonic & Knuckles cartridge " +
                          "using the wondrous Lock-On Technology™, it brought you to a secret standalone version " +
                          "of the \"Get Blue Spheres\" minigame from Sonic The Hedgehog 3.\n\n" +
                          "Play an almost infinite stream of procedurally generated levels, collect all the Blue Spheres and get hints!";
            SupportedVersions.Add("Sonic & Knuckles + Sonic the Hedgehog (World)");
            SupportedVersions.Add("Blue Spheres Plus");
            SupportedEmulators.Add("Bizhawk 2.9.1 (Genesis Plus GX core)");
            CoverFilename = "sonic_blue_spheres.png";
            Author = "Dinopony";

            Quests.Add(_levelsQuest);
            Quests.Add(_ringsQuest);
        }

        public override bool Connect()
        {
            base.Connect();

            // Sonic & Knuckles + Sonic 1 signature
            byte[] SKS1_SIG = new byte[] { 0x4D, 0x53, 0x4B, 0x26 };
            uint SKS1_ADDR = 0xFFFC;
            if (FindRamSignature(SKS1_SIG, SKS1_ADDR))
                return true;

            // Blue Spheres Plus signature
            byte[] BLUE_SPHERES_PLUS_SIG = new byte[] { 0x4C, 0x04, 0xF9, 0x4E };
            uint BLUE_SPHERES_PLUS_ADDR = 0xFFF4;
            if (FindRamSignature(BLUE_SPHERES_PLUS_SIG, BLUE_SPHERES_PLUS_ADDR))
                return true;

            return false;
        }

        public override bool Poll()
        {
            ushort blueSpheresWinAnimProgress = _ram.ReadUint16(_megadriveRamBaseAddr + 0xE44A);
            if (blueSpheresWinAnimProgress > 0 && _previousBlueSpheresWinAnimProgress == 0)
                _levelsQuest.CurrentValue += 1;
            _previousBlueSpheresWinAnimProgress = blueSpheresWinAnimProgress;

            ushort collectedRings = _ram.ReadUint16(_megadriveRamBaseAddr + 0xE43A);
            _ringsQuest.UpdateValue(collectedRings);

            return true;
        }
    }
}
