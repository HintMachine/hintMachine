using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class ZachtronicsSolitaireConnector : IGameConnector
    {
        private readonly HintQuestCounter _winsQuest = new HintQuestCounter {
            Name = "Wins",
            GoalValue = 1,
            MaxIncrease = 2,
            Description = "Fortune's Foundation wins are worth double"
        };

        private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private string _fileToReadOnNextTick = "";
        private Dictionary<string, int> _totalWinCounts = new Dictionary<string, int>();
        private ProcessRamWatcher _ram = null;

        // ----------------------------------------------------------------------------------

        public ZachtronicsSolitaireConnector()
        {
            Name = "Zachtronics Solitaire Collection";
            Description = "Play 8 different variants of Solitaire in this collection of games " +
                          "initially created as mini-games for Zachtronics main titles.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            SupportedVersions.Add("GOG");
            SupportedVersions.Add("itch.io");
            CoverFilename = "zachtronics_solitaire_collection.png";
            Author = "Dinopony";

            Quests.Add(_winsQuest);
        }

        protected override bool Connect()
        {
            try
            {
                _ram = new ProcessRamWatcher("The Zachtronics Solitaire Collection");
                if (!_ram.TryConnect())
                    return false;

                // Setup a watcher to be notified when the file is changed
                foreach(string pathToDir in FindPotentialSavefileDirectories())
                {
                    FileSystemWatcher watcher = new FileSystemWatcher
                    {
                        Path = pathToDir,
                        NotifyFilter = NotifyFilters.LastWrite,
                        Filter = "save.dat",
                        EnableRaisingEvents = true
                    };

                    string pathToFile = pathToDir + "/save.dat";
                    _totalWinCounts[pathToFile] = ReadTotalWinCount(pathToFile);

                    watcher.Changed += new FileSystemEventHandler((object source, FileSystemEventArgs e) => { 
                        _fileToReadOnNextTick = pathToFile; 
                    });

                    _watchers.Add(watcher);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Disconnect()
        {
            foreach (FileSystemWatcher watcher in _watchers)
                watcher.EnableRaisingEvents = false;
            _watchers.Clear();

            _ram = null;
        }

        protected override bool Poll()
        {
            if(!_ram.TestProcess())
                return false;

            if (_fileToReadOnNextTick != "")
            {
                int oldWins = _totalWinCounts[_fileToReadOnNextTick];
                int newWins = ReadTotalWinCount(_fileToReadOnNextTick);
                if (newWins > oldWins)
                    _winsQuest.CurrentValue += (newWins - oldWins);
                _totalWinCounts[_fileToReadOnNextTick] = newWins;

                _fileToReadOnNextTick = "";
            }

            return true;
        }

        private int ReadTotalWinCount(string pathToFile)
        {
            int totalWinCount = 0;

            FileStream file = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(file, Encoding.UTF8))
            {
                string text = streamReader.ReadToEnd();
                string[] lines = text.Split('\n');

                foreach (string line in lines)
                {
                    Regex regex = new Regex(" = ");
                    string[] keyval = regex.Split(line);
                    if (keyval.Length < 2)
                        continue;

                    if (keyval[0] == "Milan.WinCount")
                        totalWinCount += int.Parse(keyval[1]) * 2; // Fortune's Foundation wins are worth double
                    else if (keyval[0].Contains("WinCount"))
                        totalWinCount += int.Parse(keyval[1]);
                }
            }

            file.Close();

            return totalWinCount;
        }

        private string[] FindPotentialSavefileDirectories()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += @"\My Games\The Zachtronics Solitaire Collection\";
            return Directory.GetDirectories(path);
        }
    }
}
