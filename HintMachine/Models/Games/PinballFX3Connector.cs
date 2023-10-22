using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    public class PinballFX3Connector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "Pinball FX3",
            Hash = "3C0523526FF96AAA02E35BB0AB644FC1EDAC236C057A529EBD28EF8AFC3B9469"
        };

        private readonly HintQuestCumulative _scoreQuest = new HintQuestCumulative
        {
            Name = "Score",
            Description = "Any Table: Single Player, Classic Single Player",
            GoalValue = 5000000,
        };

        private ProcessRamWatcher _ram = null;

        public PinballFX3Connector()
        {
            Name = "Pinball FX3";
            Description = "Pinball FX3 is the biggest, most community focused pinball game ever created. Multiplayer matchups, user generated tournaments and league play create endless opportunity for pinball competition.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "pinball_fx3.png";
            Author = "Serpent.AI";

            Quests.Add(_scoreQuest);
        }

        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(GAME_VERSION_STEAM);
            _ram.Is64Bit = false;

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }

        protected override bool Poll()
        {
            long competitionHandlerStructAddress = _ram.ResolvePointerPath32(_ram.BaseAddress + 0x260E18, new int[] { 0x48, 0x8C, 0x8, 0x18, 0x28, 0x0 });

            if (competitionHandlerStructAddress != 0)
            {
                try
                {
                    long scoreValue = _ram.ReadUint32(competitionHandlerStructAddress + 0x50);
                    _scoreQuest.UpdateValue(scoreValue);
                }
                catch { }
            }

            return true;
        }
    }
}