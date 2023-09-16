namespace HintMachine.Games
{
    public class SonicBlueSpheresConnector : IMegadriveConnector
    {
        private bool _canWin = true;
        private readonly HintQuest _levelsQuest = new HintQuest("Completed Levels", 2);

        private bool _canPerfect = true;
        private ushort _previousCollectedRings = 0;
        private readonly HintQuest _perfectsQuest = new HintQuest("Perfect Levels", 1);

        public SonicBlueSpheresConnector() : base()
        {
            quests.Add(_levelsQuest);
            quests.Add(_perfectsQuest);
        }

        public override string GetDisplayName()
        {
            return "Sonic 3 Blue Spheres";
        }
        public override string GetDescription()
        {
            return "When you plugged the Sonic The Hedgehog cartridge into the Sonic & Knuckles cartridge " +
                   "using the wondrous Lock-On Technology™, it brought you to a secret standalone version " +
                   "of the \"Get Blue Spheres\" minigame from Sonic The Hedgehog 3.\n\n" +
                   "Play an almost infinite stream of procedurally generated levels, collect all the Blue Spheres and get hints!\n\n" +
                   "Emulator: Bizhawk (using default Genesis Plus GX core)\n" +
                   "ROM: 'Sonic & Knuckles + Sonic the Hedgehog' or 'Blue Spheres Plus'";
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
            if (blueSpheresWinAnimProgress > 0 && _canWin)
            {
                _levelsQuest.Add(1);
                _canWin = false; // We got our win once, no need to trigger it again
            }
            else
            {
                ushort remainingBlueSpheres = _ram.ReadUint16(_megadriveRamBaseAddr + 0xE438);
                if (remainingBlueSpheres > 0)
                    _canWin = true; // There are blue spheres, that means we are playing and can win again
            }

            ushort collectedRings = _ram.ReadUint16(_megadriveRamBaseAddr + 0xE43A);
            if(collectedRings > _previousCollectedRings)
            {
                ushort remainingRings = _ram.ReadUint16(_megadriveRamBaseAddr + 0xE442);
                if (remainingRings == 0 && _canPerfect)
                {
                    _perfectsQuest.Add(1);
                    _canPerfect = false; // We got our perfect once, no need to trigger it again
                }
                else if (remainingRings > 0)
                {
                    _canPerfect = true; // There are rings, that means we are playing in a level where we can perfect again
                }
            }
            _previousCollectedRings = collectedRings;

            return true;
        }
    }
}
