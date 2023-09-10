using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HintMachine.Games
{
    public class ZachtronicsSolitaireConnector : IGameConnector
    {
        private long _pollCount = 0;

        private int _previousWins = int.MaxValue;
        private readonly HintQuest _winsQuest = new HintQuest("Wins", 2);

        public ZachtronicsSolitaireConnector()
        {
            quests.Add(_winsQuest);
        }

        public override bool Connect()
        {
            try
            {
                FileStream file = new FileStream(FindSavefilePath(), FileMode.Open, FileAccess.Read);
                file.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Disconnect() {}

        public override string GetDisplayName()
        {
            return "Zachtronics Solitaire Collection (GOG)";
        }

        public override void Poll()
        {
            // Only read the file once every 20 ticks (every 2 seconds)
            if (++_pollCount % 20 != 0)
                return;

            int totalWinCount = ReadTotalWinCount();
            if (totalWinCount > _previousWins)
                _winsQuest.Add(totalWinCount - _previousWins);
            _previousWins = totalWinCount;
        }

        private int ReadTotalWinCount()
        {
            int totalWinCount = 0;
            try
            {
                FileStream file = new FileStream(FindSavefilePath(), FileMode.Open, FileAccess.Read);
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

                        if (keyval[0].Contains("WinCount"))
                            totalWinCount += int.Parse(keyval[1]);
                    }
                }

                file.Close();
            }
            catch {}
     
            return totalWinCount;
        }

        private string FindSavefilePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += @"\My Games\The Zachtronics Solitaire Collection\NonSteamUser\save.dat";
            return path;
        }
    }
}
