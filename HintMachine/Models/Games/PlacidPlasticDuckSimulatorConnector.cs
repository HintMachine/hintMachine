using System;
using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;


namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class PlacidPlasticDuckSimulatorConnector : IGameConnector
    {
        private readonly BinaryTarget GAME_VERSION_STEAM = new BinaryTarget
        {
            DisplayName = "Steam",
            ProcessName = "Placid Plastic Duck Simulator",
            ModuleName = "UnityPlayer.dll",
            Hash = "46BDBD3FF415B7F8F4FDA79B6E8DB403CE87DDB1614E407C911400789F1213DD"
        };

        private readonly HintQuestCumulative _timerQuest = new HintQuestCumulative
        {
            Name = "Spawn Timer (Seconds Elapsed)",
            GoalValue = 180,
        };

        private readonly HintQuestCumulative _duckQuest = new HintQuestCumulative
        {
            Name = "Ducks Spawned",
            GoalValue = 5, 
            MaxIncrease = 1,
        };

        private ProcessRamWatcher _ram = null;


        public PlacidPlasticDuckSimulatorConnector()
        {
            Name = "Placid Plastic Duck Simulator";
            Description = "The ultimate high-tech rubber duck simulation, Placid Plastic Duck brings you dangerous levels of relaxation. With chill music, dreamy 3D graphics, and many different happy ducks, your only priority is to float around. Zero Ducks given.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "placid_plastic_duck_simulator.png";
            Author = "Serpent.AI";

            Quests.Add(_timerQuest);
            Quests.Add(_duckQuest);
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
            long generalManagerStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x19CD340, new int[] { 0xD0, 0x8, 0x60, 0x20, 0x0 });
            long duckListStructAddress = _ram.ResolvePointerPath64(_ram.BaseAddress + 0x19CD340, new int[] { 0xD0, 0x8, 0x60, 0x20, 0x98, 0x0 });

            if (generalManagerStructAddress != 0 && duckListStructAddress != 0)
            {
                try
                {
                    long spawnStopValue = _ram.ReadUint32(generalManagerStructAddress + 0x1E4);
                    float spawnCounterValue = _ram.ReadFloat(generalManagerStructAddress + 0x1E8);

                    long duckCountValue = _ram.ReadUint32(duckListStructAddress + 0x18);

                    if (spawnStopValue == 0)
                    {
                        _timerQuest.UpdateValue((long)Math.Round(spawnCounterValue));

                        if (duckCountValue > 0)
                        {
                            _duckQuest.UpdateValue(duckCountValue);
                        }
                        
                    }
                }
                catch { }
            }

            return true;
        }
    }
}