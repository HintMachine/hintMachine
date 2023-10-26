using System;
using System.Collections.Generic;

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
            Name = "Score - Select a table...",
            Description = "Any Table: Single Player, Classic Single Player",
            GoalValue = 10000000,
        };

        private ProcessRamWatcher _ram = null;

        struct TableData
        {
            public string Name;
            public long TargetScore;

            public TableData(string name, long targetScore)
            {
                Name = name;
                TargetScore = targetScore;
            }
        }

        private Dictionary<long, TableData> _tableDataMapping = new Dictionary<long, TableData>
        {
            { 80, new TableData("Son of Zeus", 7200000) },
            { 29, new TableData("Adventure Land", 5400000) },
            { 75, new TableData("Wild West Rampage", 2700000) },
            { 40, new TableData("Castle Storm", 7200000) },
            { 61, new TableData("El Dorado", 3600000) },
            { 32, new TableData("Shaman", 3600000) },
            { 34, new TableData("Tesla", 2700000) },
            { 26, new TableData("V12", 2250000) },
            { 67, new TableData("Epic Quest", 4500000) },
            { 20, new TableData("Excalibur", 4500000) },
            { 33, new TableData("Sorcerer's Lair", 6300000) },
            { 52, new TableData("Earth Defense", 4500000) },
            { 70, new TableData("Mars", 9000000) },
            { 9, new TableData("Paranormal", 1800000) },
            { 27, new TableData("Biolab", 6300000) },
            { 23, new TableData("Pasha", 4500000) },
            { 31, new TableData("Rome", 2700000) },
            { 66, new TableData("Secrets of the Deep", 2700000) },
            { 133, new TableData("Indiana Jones - The Pinball Adventure", 54000000) },
            { 134, new TableData("Funhouse", 2700000) },
            { 129, new TableData("Space Station", 450000) },
            { 135, new TableData("Dr. Dude and his Excellent X-Ray", 2700000) },
            { 127, new TableData("Cirqus Voltaire", 7200000) },
            { 128, new TableData("No Good Gofers", 3600000) },
            { 132, new TableData("Tales of the Arabian Nights", 2700000) },
            { 130, new TableData("Monster Bash", 45000000) },
            { 131, new TableData("The Creature from the Black Lagoon", 54000000) },
            { 124, new TableData("White Water", 45000000) },
            { 125, new TableData("Red and Ted's Road Show", 108000000) },
            { 126, new TableData("Hurricane", 9000000) },
            { 117, new TableData("Theatre of Magic", 180000000) },
            { 121, new TableData("The Champion Pub", 6300000) },
            { 122, new TableData("Safe Cracker", 450000) },
            { 118, new TableData("Black Rose", 9000000) },
            { 119, new TableData("Attack from Mars", 1350000000) },
            { 120, new TableData("The Party Zone", 11700000)},
            { 108, new TableData("Fish Tales", 150000000)},
            { 111, new TableData("The GetaWay: High Speed II", 28800000)},
            { 110, new TableData("Junk Yard", 8100000)},
            { 109, new TableData("Medieval Madness", 9000000)},
            { 101, new TableData("Jurassic World Pinball", 4500000)},
            { 100, new TableData("Jurassic Park Pinball", 13860000)},
            { 102, new TableData("Jurassic Park Pinball Mayhem", 5400000)},
            { 97, new TableData("Back to the Future Pinball", 13500000)},
            { 95, new TableData("Jaws Pinball", 5400000)},
            { 96, new TableData("E.T. Pinball", 6300000)},
            { 93, new TableData("Marvel's Women of Power: A-Force", 2700000)},
            { 94, new TableData("Marvel's Women of Power: Champions", 9000000)},
            { 79, new TableData("Marvel's Ant-Man", 9000000)},
            { 78, new TableData("Marvel's Avengers: Age of Ultron", 9000000)},
            { 50, new TableData("Marvel's Guardians of the Galaxy", 9000000)},
            { 73, new TableData("Marvel's Venom", 2700000)},
            { 22, new TableData("Marvel's Deadpool", 5400000)},
            { 1, new TableData("Marvel's Civil War", 3600000)},
            { 30, new TableData("Marvel's Doctor Strange", 13500000)},
            { 48, new TableData("Marvel's Captain America", 9000000)},
            { 38, new TableData("Marvel's Fantastic Four", 5400000)},
            { 56, new TableData("Marvel's World War Hulk", 6300000)},
            { 5, new TableData("Marvel's Fear Itself", 4500000)},
            { 60, new TableData("Marvel's The Infinity Gauntlet", 7650000)},
            { 17, new TableData("Marvel's The Avengers", 2700000)},
            { 21, new TableData("Marvel's Ghost Rider", 4500000)},
            { 7, new TableData("Marvel's Thor", 4500000)},
            { 59, new TableData("Marvel's X-Men", 6300000)},
            { 49, new TableData("Marvel's Moon Knight", 3150000)},
            { 65, new TableData("Marvel's The Invincible Iron Man", 3600000)},
            { 46, new TableData("Marvel's The Amazing Spider-Man", 6300000)},
            { 45, new TableData("Marvel's Blade", 3600000)},
            { 71, new TableData("Marvel's Wolverine", 6300000)},
            { 105, new TableData("Star Wars Pinball: Solo", 13500000)},
            { 106, new TableData("Star Wars Pinball: Calrissian Chronicles", 6750000)},
            { 107, new TableData("Star Wars Pinball: Battle of Mimban", 9000000)},
            { 103, new TableData("Star Wars Pinball: The Last Jedi", 22500000)},
            { 104, new TableData("Star Wars Pinball: Ahch-To Island", 13500000)},
            { 98, new TableData("Star Wars Pinball: Rogue One", 5400000)},
            { 77, new TableData("Star Wars Pinball: Star Wars Rebels", 7650000)},
            { 88, new TableData("Star Wars Pinball: The Force Awakens", 9000000)},
            { 89, new TableData("Star Wars Pinball: Might of the First Order", 3600000)},
            { 10, new TableData("Star Wars Pinball: Han Solo", 2250000)},
            { 58, new TableData("Star Wars Pinball: Droids", 2250000)},
            { 12, new TableData("Star Wars Pinball: Episode IV A New Hope", 8100000)},
            { 2, new TableData("Star Wars Pinball: Masters of the Force", 2250000)},
            { 24, new TableData("Star Wars Pinball: Episode VI Return of the Jedi", 4050000)},
            { 28, new TableData("Star Wars Pinball: Darth Vader", 9000000)},
            { 6, new TableData("Star Wars Pinball: Starfighter Assault", 3600000)},
            { 43, new TableData("Star Wars Pinball: Episode V The Empire Strikes Back", 8100000)},
            { 47, new TableData("Star Wars Pinball: The Clone Wars", 6300000)},
            { 53, new TableData("Star Wars Pinball: Boba Fett", 4500000)},
            { 90, new TableData("Fallout Pinball", 6300000)},
            { 92, new TableData("DOOM Pinball", 8100000)},
            { 91, new TableData("The Elder Scrolls V: Skyrim Pinball", 9000000)},
            { 85, new TableData("Aliens Pinball", 6300000)},
            { 86, new TableData("Alien vs. Predator Pinball", 3600000)},
            { 87, new TableData("Alien: Isolation Pinball", 3600000)},
            { 82, new TableData("Family Guy Pinball", 7200000)},
            { 83, new TableData("Bob's Burgers Pinball", 10800000)},
            { 84, new TableData("Archer Pinball", 5400000)},
            { 81, new TableData("American Dad! Pinball", 9000000)},
            { 76, new TableData("Portal Pinball", 4500000)},
            { 72, new TableData("The Walking Dead Pinball", 5400000)},
        };

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
                    long tableIdValue = _ram.ReadUint32(competitionHandlerStructAddress + 0x80);
                    long scoreValue = (long)_ram.ReadUint64(competitionHandlerStructAddress + 0x50);

                    if (_tableDataMapping.TryGetValue(tableIdValue, out TableData tableData))
                    {
                        double scoringMultiplier = Math.Round(1.0 / (tableData.TargetScore / 10000000.0), 3);

                        _scoreQuest.Name = $"Score X {scoringMultiplier} ({tableData.Name})";
                        _scoreQuest.UpdateValue((long)(scoreValue * scoringMultiplier));
                    }
                }
                catch { }
            }
            else
            {
                _scoreQuest.Name = $"Score - Select a table...";
            }

            return true;
        }
    }
}