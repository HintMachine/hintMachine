using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class PaintTheTownRedConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "PaintTheTownRed",
            ModuleName = "GameAssembly.dll",
            Hash = "D2233BEEA6BC9EA0CE0B2D356BE6851FF4599B8565323A584C01E6940C857DBB"
        };

        private readonly HintQuestCumulative _beneathGoldQuest = new HintQuestCumulative
        {
            Name = "Beneath: Gold Collected",
            GoalValue = 1000,
        };

        private readonly HintQuestCumulative _beneathEnergyQuest = new HintQuestCumulative
        {
            Name = "Beneath: Energy Spent in Shops",
            GoalValue = 25,
        };

        private ProcessRamWatcher _ram = null;

        public PaintTheTownRedConnector()
        {
            Name = "Paint the Town Red";
            Description = "Paint the Town Red is a chaotic first person melee combat game set in different locations and time periods and featuring a massive Rogue-Lite adventure. The voxel-based enemies can be punched, bashed, kicked, stabbed and sliced completely dynamically using almost anything that isn't nailed down.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "paint_the_town_red.png";
            Author = "Serpent.AI";

            Quests.Add(_beneathGoldQuest);
            Quests.Add(_beneathEnergyQuest);
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
            long beneathStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x2B36000, new int[] { 0xB8, 0x900 });

            if (beneathStructAddress != 0)
            {
                try
                {
                    long goldValue = _ram.ReadUint32(beneathStructAddress + 0xB4);
                    long energyValue = _ram.ReadUint32(beneathStructAddress + 0xBC);

                    _beneathGoldQuest.UpdateValue(goldValue);
                    _beneathEnergyQuest.UpdateValue(energyValue);
                }
                catch { }
            }

            return true;
        }
    }
}
