using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HintMachine.Games
{
    public class OneFingerDeathPunchConnector : IGameConnectorProcess
    {
        private long _previousKills = long.MaxValue;
        private readonly HintQuest _killsQuest = new HintQuest("Kills", 450);
        IntPtr Thread0Address;
        public OneFingerDeathPunchConnector() : base("One Finger Death Punch")
        {
            quests.Add(_killsQuest);
        }

        public override string GetDisplayName()
        {
            return "One Finger Death Punch";
        }

        public override string GetDescription()
        {
            return "Fight using only your left and right mouse button. Your cursor can be anywhere on the screen" +
                "Tested on up-to-date Steam version";
        }

        public override bool Poll()
        {
            if (process == null || module == null)
                return false;

            syncThreadStackAdr();
            long killsAddress = ResolvePointerPath32(Thread0Address.ToInt64() - 0x8cc, new int[] { 0x644, 0x90 });

            uint kills = ReadUint32(killsAddress);
            if (kills > _previousKills)
                _killsQuest.Add(kills - _previousKills);
            _previousKills = kills;
            return true;
        }

        private async void syncThreadStackAdr() {
            Thread0Address = (IntPtr)await getThread0Address();
        }

        private Task<int> getThread0Address()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "threadstack.exe",
                    Arguments = process.Id + "",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.Contains("THREADSTACK 0 BASE ADDRESS: "))
                {
                    line = line.Substring(line.LastIndexOf(":") + 2);
                    return Task.FromResult(int.Parse(line.Substring(2), System.Globalization.NumberStyles.HexNumber));
                }
            }
            return Task.FromResult(0);
        }
    }
}
