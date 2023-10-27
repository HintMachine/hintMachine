using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class OneFingerDeathPunchConnector : IGameConnector
    {
        private readonly HintQuestCumulative _killsQuest = new HintQuestCumulative
        {
            Name = "Kills",
            GoalValue = 450,
            Description = "Kill enemies in story mode or survival to progress",
            MaxIncrease = 100,
            Direction = HintQuestCumulative.CumulativeDirection.ASCENDING
            
        };

        private ProcessRamWatcher _ram = null;

        public OneFingerDeathPunchConnector()
        {
            Name = "One Finger Death Punch";
            Description = "Experience cinematic kung-fu battles in the fastest, most intense brawler the indie world has ever seen! With the unique 1:1 response system of One Finger Death Punch, players will feel the immediate feedback of every bone-crunching hit.\r\n\r\nPay tribute to the masters using five classic kung-fu styles mixed with additional weapons. Combine face-to-face combat with throwing weapons to recreate complex fight choreographies or just send bad guys flying through glass windows. Explore a world map with over 250 stages, 13 modes, and 3 difficulty levels. Unlock 21 different skills that can be combined in thousands of ways to assist you in your journey. Put your kung-fu to the ultimate test in the survival mode.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "one_finger_death_punch.png";
            Author = "CalDrac";

            Quests.Add(_killsQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher("One Finger Death Punch");
            _ram.Is64Bit = false;

            if (!_ram.TryConnect())
                return false;

            return true;
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            //long killsAddress = _ram.ResolvePointerPath32(_threadstack0Address.ToInt32() - 0x8cc, new int[] { 0x644, 0x90 });
            long killsAddress = _ram.ResolvePointerPath32(_ram.Threadstack0 - 0x8A0, new int[] { 0x710, 0x3C });
            _killsQuest.UpdateValue(_ram.ReadUint32(killsAddress));

            return true;
        }
    }
}
