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

        public static Settings Default { get {  return new Settings(); } }

        public static void SaveToFile()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>() {
                { "host", Host },
                { "slot", Slot },
                { "game", Game }
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
                }
            }
            catch { }
        }
    }
}
