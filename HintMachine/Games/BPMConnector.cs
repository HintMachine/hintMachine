using HintMachine.GenericConnectors;

namespace HintMachine.Games
{
    public class BPMConnector : IGameConnector
    {
        private readonly HintQuestCumulative _enemiesKilledQuest = new HintQuestCumulative
        {
            Name = "Enemies killed",
            GoalValue = 80,
            MaxIncrease = 5
        };

        private readonly HintQuestCumulative _goldSpentQuest = new HintQuestCumulative
        {
            Name = "Gold spent in shops",
            GoalValue = 40,
            MaxIncrease = 30
        };

        private ProcessRamWatcher _ram = null;

        // -----------------------------------------------------------------------

        public BPMConnector()
        {
            Name = "BPM: Bullets Per Minute";
            Description = "BPM: Bullets Per Minute is a rhythm-based FPS roguelite.\n\nIn BPM, all of your actions and the actions of your enemies are tied to the beat of the music. Your enemies perform a dance-like sequence of attacks to an epic rock opera. BPM is inspired by retro shooters of the 90’s. It is fast, frenetic and rhythmical. You can double jump, dash, rocket jump and bunny hop to evade your opponents.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "bpm.png";
            Author = "Dinopony";

            Quests.Add(_enemiesKilledQuest);
            Quests.Add(_goldSpentQuest);
        }

        public override bool Connect()
        {
            _ram = new ProcessRamWatcher("BPMGame-Win64-Shipping");
            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        public override bool Poll()
        {
            long lifetimeStatsAddr = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x4952D20, new int[] { 0x288, 0x158 });
            if (lifetimeStatsAddr != 0)
            {
                uint enemiesKilled = _ram.ReadUint32(lifetimeStatsAddr + 0x14);
                _enemiesKilledQuest.UpdateValue(enemiesKilled);

                uint goldSpentHuggins = _ram.ReadUint32(lifetimeStatsAddr);
                uint goldSpentMunnin = _ram.ReadUint32(lifetimeStatsAddr + 0x4);
                _goldSpentQuest.UpdateValue(goldSpentHuggins + goldSpentMunnin);
            }   

            return true;
        }
    }
}