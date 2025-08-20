using System;
using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class PixTheCatConnector : IGameConnector
    {
        private readonly BinaryTarget _gameVersionSteam = new()
        {
            DisplayName = "Steam",
            ProcessName = "PixTheCat",
            Hash = "89DB207F57366E3C6C1FF9BDBB24AE9388DF228D113FCF146FCCB344DBF191C1"
        };
        
        private readonly HintQuestCumulative _scoreQuest = new()
        {
            Name = "300,000 Points gained",
            Description = "Select \"Arcade - Main grid\" mode and have fun!",
            GoalValue = 300000,
        };
        
        private readonly int[] _pointerPaths = new int[] { 0x188, 0x428, 0x238, 0xD4 };
        
        private ProcessRamWatcher _ram = null;
        
        public PixTheCatConnector()
        {
            Name = "Pix the Cat";
            Description = "Pix the Cat is an intense, critically acclaimed arcade game that will test your reflexes and wits, pitting you against your friends and the world to achieve the highest score.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "pix_the_cat.png";
            Author = "Gomiken";

            Quests.Add(_scoreQuest);
        }
        
        protected override bool Connect()
        {
            _ram = new ProcessRamWatcher(_gameVersionSteam)
            {
                Is64Bit = false
            };

            return _ram.TryConnect();
        }

        public override void Disconnect()
        {
            _ram = null;
        }


        protected override bool Poll()
        {
            var address = _ram.ResolvePointerPath32(_ram.BaseAddress + 0x00D3A488, _pointerPaths);
            if (address == 0) return true;

            try
            {
                _scoreQuest.UpdateValue(_ram.ReadInt32(address));
            }
            catch (Exception ignored)
            {
                // ignored
            }

            return true;
        }
    }
}