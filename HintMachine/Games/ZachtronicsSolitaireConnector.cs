using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HintMachine.Games
{
    public class ZachtronicsSolitaireConnector : IGameConnector
    {
        private int _previousWins = int.MaxValue;
        private readonly HintQuest _winsQuest = new HintQuest("Wins", 2);

        FileSystemWatcher _watcher = null;
        private bool _readSaveFileOnNextTick = false;

        public ZachtronicsSolitaireConnector()
        {
            quests.Add(_winsQuest);
        }

        public override bool Connect()
        {
            try
            {
                // Read the file a first time to have the initial win count
                _previousWins = ReadTotalWinCount();

                // Setup a watcher to be notified when the file is changed
                _watcher = new FileSystemWatcher();
                _watcher.Path = FindSavefileDirectory();
                _watcher.NotifyFilter = NotifyFilters.LastWrite;
                _watcher.Filter = "save.dat";
                _watcher.Changed += new FileSystemEventHandler(OnFileChanged);
                _watcher.EnableRaisingEvents = true;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Disconnect()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher = null; 
        }

        public override string GetDisplayName()
        {
            return "Zachtronics Solitaire Collection (GOG)";
        }

        public override bool Poll()
        {
            if (_readSaveFileOnNextTick)
            {
                try
                {
                    int totalWinCount = ReadTotalWinCount();
                    if (totalWinCount > _previousWins)
                        _winsQuest.Add(totalWinCount - _previousWins);
                    _previousWins = totalWinCount;
                }
                catch 
                {
                    return false; 
                }

                _readSaveFileOnNextTick = false;
            }

            return true;
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            _readSaveFileOnNextTick = true;
        }

        private int ReadTotalWinCount()
        {
            int totalWinCount = 0;

            FileStream file = new FileStream(FindSavefileDirectory() + "save.dat", FileMode.Open, FileAccess.Read);
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

        private string FindSavefileDirectory()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += @"\My Games\The Zachtronics Solitaire Collection\NonSteamUser\";
            return path;
        }
    }
}
