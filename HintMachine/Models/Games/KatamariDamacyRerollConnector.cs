using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class KatamariDamacyRerollConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "katamari",
            ModuleName = "PS2KatamariSimulation.dll",
            Hash = "4E1E402B8E66F04E12688A8DD9BE80A061E8DCD7B1A368FAAECA151906474767"
        };

        private readonly HintQuestCumulative _objectQuest = new HintQuestCumulative
        {
            Name = "Objects Collected",
            GoalValue = 250,
        };

        private readonly HintQuestCumulative _constellationQuest = new HintQuestCumulative
        {
            Name = "Constellation Objects",
            GoalValue = 100,
        };

        private ProcessRamWatcher _ram = null;

        public KatamariDamacyRerollConnector()
        {
            Name = "Katamari Damacy REROLL";
            Description = "The stop-at-nothing pushing prince is back and ready to reroll! When the King of All Cosmos accidentally destroys all the stars in the sky, he orders you, his pint-sized princely son, to put the twinkle back in the heavens above. Join the King and Prince of Cosmos on their wacky adventure to restore the stars – now in full HD!\r\n\r\nThe beloved roll-em-up game returns with fully updated graphics, completely recreated cutscenes and in full HD!";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "katamari_damacy_reroll.png";
            Author = "Serpent.AI";

            Quests.Add(_objectQuest);
            Quests.Add(_constellationQuest);
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
            try
            {
                long objectValue = _ram.ReadUint32(_ram.BaseAddress + 0x150EE8);
                long constellationValue = _ram.ReadUint32(_ram.BaseAddress + 0x150EEC);

                if (objectValue <= 5000) { _objectQuest.UpdateValue(objectValue); }
                if (constellationValue <= 5000) { _constellationQuest.UpdateValue(constellationValue); }
            }
            catch { }

            return true;
        }
    }
}
