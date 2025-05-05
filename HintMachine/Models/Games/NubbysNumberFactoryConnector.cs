using HintMachine.Helpers;
using HintMachine.Models.GenericConnectors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Printing.IndexedProperties;
using System.Text;
using System.Text.RegularExpressions;

namespace HintMachine.Models.Games
{
    [AvailableGameConnector]
    public class NubbysNumberFactoryConnector : IGameConnector
    {
        private ProcessRamWatcher _ram = null;
        private readonly HintQuestCumulative _WinQuest = new HintQuestCumulative
        {
            Name = "Win",
            GoalValue = 1,
            MaxIncrease = 2,
            Description = "Challenge Wins are worth double"
        };
        protected List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private string _fileToReadOnNextTick = "";
        protected Dictionary<string, int> _totalWinCounts = new Dictionary<string, int>();
        public NubbysNumberFactoryConnector()
        {
            Name = "Nubby's Number Factory";
            Description = "A plinko-style roguelike where you work at a number factory. Launch your spherical friend, Nubby, down a pegboard to make bigger and bigger numbers! But beware, if you fail to meet your number quota, the sun will explode.";
            Platform = "PC";
            SupportedVersions.Add("Steam");
            CoverFilename = "nubbysnumberfactory.png";
            Author = "MogDogBlog";

            Quests.Add(_WinQuest);
        }
		 private int ReadTotalWinCount(string pathToFile)
        {
            int totalWinCount = 0;

            FileStream file = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(file, Encoding.UTF8))
            {
                string text = streamReader.ReadToEnd();
                string[] lines = text.Split(',');

                foreach (string line in lines)
                {
                    Regex regex = new Regex(":");
                    string[] keyval = regex.Split(line);
                    String stripped = Regex.Replace(keyval[1], @"[^0-9\.]","");
                    keyval[1] = stripped;
                    Debug.WriteLine(stripped);
                    if (keyval.Length < 2)
                        continue;
                    if (keyval[0].Contains("SaveChWins"))
                        totalWinCount += (int)float.Parse(keyval[1]) * 2; // Double Challenge Wins
                    else if (keyval[0].Contains("SaveSvWins"))
                        totalWinCount += (int)float.Parse(keyval[1]);
                }
            }

            file.Close();

            return totalWinCount;
        }

        protected override bool Connect()
        {
           try
            {
                _ram = new ProcessRamWatcher("NNF_FULLVERSION");
                Debug.Write("Connected");
                if (!_ram.TryConnect())
                    return false;
                // Setup a watcher to be notified when the file is changed

                string pathToDir = FindSaveData();
                string pathToFile = pathToDir + @"\NUBBY_Progression_F.save";
                FileSystemWatcher watcher = new FileSystemWatcher(pathToDir)
                    {
                        Filter = "",
                        NotifyFilter = NotifyFilters.LastAccess |
                        NotifyFilters.LastWrite |
                        NotifyFilters.CreationTime,
                        EnableRaisingEvents = true  
                    };
                    
                    _totalWinCounts[pathToFile] = ReadTotalWinCount(pathToFile);
                    Debug.WriteLine(pathToFile + " " + _totalWinCounts[pathToFile]);
                    watcher.Changed += new FileSystemEventHandler((object source, FileSystemEventArgs e) => {
                        Debug.WriteLine("Save Changed");
                        _fileToReadOnNextTick = pathToFile;
                    });

                _watchers.Add(watcher);
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
                Debug.WriteLine("Old = " + oldWins);
                Debug.WriteLine("New = " + newWins);
                if (newWins > oldWins)
                    _WinQuest.CurrentValue += (newWins - oldWins);
                _totalWinCounts[_fileToReadOnNextTick] = newWins;

                _fileToReadOnNextTick = "";
            }


            return true;
        }
		private string FindSaveData(){
			string path = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            path += @"\NNF_FULLVERSION";
            Console.WriteLine(path);
            return path;
		}
    }
}