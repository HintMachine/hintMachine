using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HintMachine
{
    internal class Settings
    {
        public static string Host = "archipelago.gg:12345";
        public static string Slot = "";
        public static string Game = "";
        public static bool DisplayChatMessages = true;
        public static bool DisplayItemNotificationMessages = true;
        public static bool DisplayFoundHintMessages = false;
        public static bool DisplayJoinLeaveMessages = false;

        public static void SaveToFile()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>() {
                { "host",                            Host },
                { "slot",                            Slot },
                { "game",                            Game },
                { "displayChatMessages",             DisplayChatMessages.ToString() },
                { "displayItemNotificationMessages", DisplayItemNotificationMessages.ToString() },
                { "displayFoundHintMessages",        DisplayFoundHintMessages.ToString() },
                { "displayJoinLeaveMessages",        DisplayJoinLeaveMessages.ToString() },
            };
            File.WriteAllLines("settings.cfg", dict.Select(x => x.Key + "=" + x.Value).ToArray());
        }

        public static void LoadFromFile()
        {
            try
            {
                string[] lines = File.ReadAllLines("settings.cfg");
                foreach (var line in lines)
                {
                    int idx = line.IndexOf('=');
                    if (idx == -1)
                        continue;

                    string value = line.Substring(idx + 1);
                    if (line.StartsWith("host"))
                        Host = value;
                    else if (line.StartsWith("slot"))
                        Slot = value;
                    else if (line.StartsWith("game"))
                        Game = value;
                    else if (line.StartsWith("displayChatMessages"))
                        DisplayChatMessages = bool.Parse(value);
                    else if (line.StartsWith("displayItemNotificationMessages"))
                        DisplayItemNotificationMessages = bool.Parse(value);
                    else if (line.StartsWith("displayFoundHintMessages"))
                        DisplayFoundHintMessages = bool.Parse(value);
                    else if (line.StartsWith("displayJoinLeaveMessages"))
                        DisplayJoinLeaveMessages = bool.Parse(value);
                }
            }
            catch { }
        }
    }
}
