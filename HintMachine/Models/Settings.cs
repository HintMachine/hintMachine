using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HintMachine.Models
{
    public static class Settings
    {
        public static string Host { get; set; } = "archipelago.gg:12345";
        public static string Slot { get; set; } = "";
        public static string LastConnectedGame { get; set; } = "";
        public static bool DisplayChatMessages { get; set; } = true;
        public static bool DisplayFoundHintMessages { get; set; } = true;
        public static bool DisplayJoinLeaveMessages { get; set; } = false;
        public static bool DisplayItemReceivedMessages { get; set; } = true;
        public static bool DisplayItemSentMessages { get; set; } = false;
        public static bool PlaySoundOnHint { get; set; } = true;
        public static bool ShowUpdatePopUp { get; set; } = true;
        public static bool StreamerMode { get; set; } = false;

        // Temporary settings (not saved to file)
        public static bool ForceDebugMessagesDisplay { get; set; } = false;

        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Save all user settings to a dedicated file alongside the application (named "settings.cfg")
        /// </summary>
        public static void SaveToFile()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>() {
                { "host",                           Host },
                { "slot",                           Slot },
                { "game",                           LastConnectedGame },
                { "displayChatMessages",            DisplayChatMessages.ToString() },
                { "displayFoundHintMessages",       DisplayFoundHintMessages.ToString() },
                { "displayJoinLeaveMessages",       DisplayJoinLeaveMessages.ToString() },
                { "displayItemReceivedMessages",    DisplayItemReceivedMessages.ToString() },
                { "displayItemSentMessages",        DisplayItemSentMessages.ToString() },
                { "playSoundOnHint",                PlaySoundOnHint.ToString() },
                { "showUpdatePopUp",                ShowUpdatePopUp.ToString() },
                { "streamerMode",                StreamerMode.ToString() },
            };
            File.WriteAllLines("settings.cfg", dict.Select(x => x.Key + "=" + x.Value).ToArray());
        }

        /// <summary>
        /// Load all user settings from the dedicated file alongside the application (named "settings.cfg")
        /// </summary>
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
                        LastConnectedGame = value;
                    else if (line.StartsWith("displayChatMessages"))
                        DisplayChatMessages = bool.Parse(value);
                    else if (line.StartsWith("displayFoundHintMessages"))
                        DisplayFoundHintMessages = bool.Parse(value);
                    else if (line.StartsWith("displayJoinLeaveMessages"))
                        DisplayJoinLeaveMessages = bool.Parse(value);
                    else if (line.StartsWith("displayItemReceivedMessages"))
                        DisplayItemReceivedMessages = bool.Parse(value);
                    else if (line.StartsWith("displayItemSentMessages"))
                        DisplayItemSentMessages = bool.Parse(value);
                    else if (line.StartsWith("playSoundOnHint"))
                        PlaySoundOnHint = bool.Parse(value);
                    else if (line.StartsWith("showUpdatePopUp"))
                        ShowUpdatePopUp = bool.Parse(value);
                    else if (line.StartsWith("streamerMode"))
                        StreamerMode = bool.Parse(value);
                }
            }
            catch { }
        }
    }
}
