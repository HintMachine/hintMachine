using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class SonicBlueSpheresConnector : IMegadriveConnector
    {
        private readonly HintQuestCounter _levelsQuest = new HintQuestCounter
        {
            Name = "Completed Levels",
            GoalValue = 2,
            CooldownBetweenIncrements = 60,
        };
        private readonly HintQuestCumulative _ringsQuest = new HintQuestCumulative
        {
            Name = "Collected Rings",
            MaxIncrease = 10,
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
            CoverFilename = "sonic_blue_spheres.png";
            Author = "Dinopony";

            Quests.Add(_levelsQuest);
            Quests.Add(_ringsQuest);

            // Sonic & Knuckles + Sonic 1
            ValidROMs.Add("C9DEA194C3169AD1287C84C157E1257326B393A315B763C5CD04B48DAD4A9DEB");
            // Blue Spheres Plus
            ValidROMs.Add("06DC460F28F77B31335FC55F0B74A21925983817BD9BBD97F8CCBFBDCAA69101");
        }

        protected override bool Poll()
        {
            ushort blueSpheresWinAnimProgress = _ram.ReadUint16(RamBaseAddress + 0xE44A);
            if (blueSpheresWinAnimProgress > 0 && _previousBlueSpheresWinAnimProgress == 0)
                _levelsQuest.CurrentValue += 1;
            _previousBlueSpheresWinAnimProgress = blueSpheresWinAnimProgress;

            ushort collectedRings = _ram.ReadUint16(RamBaseAddress + 0xE43A);
            _ringsQuest.UpdateValue(collectedRings);

            return true;
        }
    }
}
