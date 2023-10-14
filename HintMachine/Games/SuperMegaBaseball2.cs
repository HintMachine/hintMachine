namespace HintMachine.Games
{
    public class SuperMegaBaseball2Connector : IGameConnector
    {

        /// Starpoints: The rate at which starpoints are gained is chiefly affected by the intersection of EGO (difficulty) set and player skill.
        /// While higher EGO means higher multiplier (up to 100x at EGO 99), that can result in LOWER Starpoints in the end if player skill can't keep up.
        /// For sake of having a baseline for pace balancing, we're using a multiplier of x35, which corresponds to an EGO just above 60 (noted ingame as "Very Hard")
        /// The bottom end of the EGO setting for consideration is EGO 40 ("Serious", multiplier x14), who get score at 40% of this rate.
        /// The top end of the EGO setting for consideration is EGO 80 ("Good Luck", multiplier x63), who get score at 180% of this rate.
        /// Other reference points for EGO settings: EGO 50 ("Hard") -> x23, EGO 60 -> x34, and EGO 70 ("Extreme") -> x47.
         
        private readonly HintQuestCumulative _starpointsQuest = new HintQuestCumulative
        {
            Name = "Starpoints",
            GoalValue = 40000,
        };

        private ProcessRamWatcher _ram = null;

        // ------------------------------------------------------------------------

        public SuperMegaBaseball2Connector()
        {
            Name = "Super Mega Baseball 2";
            Description = "The second iteration of the Arcade-Style Baseball Game. Hit some dingers!";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "super_mega_baseball_2.png";
            Author = "Kitsune Zeta";

            Quests.Add(_starpointsQuest);
        }

        //TODO: See if there's a way to specifically identify SMB2 vs SMB3 (vs SMB1 or SMB4), since they all have the same executable name. Probably some ram checks somewhere we can use to verify.
        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("supermegabaseball");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            if (!_ram.TestProcess())
                return false;

            int[] OFFSETS = new int[] { 0x20, 0x98, 0x468, 0x10, 0x20, 0x278 };
            long scoreAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x02A88FB8, OFFSETS);
            if (scoreAddress != 0)
            {
                _starpointsQuest.UpdateValue(_ram.ReadUint32(scoreAddress));

                // _perfectClearsQuest.UpdateValue(_ram.ReadUint32(linesAddress + 0xC0));
                // _combosQuest.UpdateValue(_ram.ReadUint32(combosAddress));
            }

            return true;
        }
    }
}
