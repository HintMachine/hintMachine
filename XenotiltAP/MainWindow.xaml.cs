using System;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace XenotiltAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PROCESS_WM_READ = 0x0010;
        ArchipelagoSession archipelagoSession;
        private long scoreAdress;
        private Timer _timer;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        float score = 0;
        String scoreStr = "0";
        float scoreObjectif = 200000000;
        IntPtr processHandle;
        int nbHintsSent = 0;
        private List<long> locationsToRemove = new List<long>();

        public MainWindow(ArchipelagoSession archipelagoSession, String nomJeu)
        {
            InitializeComponent();
            this.archipelagoSession = archipelagoSession;
            TokenManipulator.AddPrivilege("SeDebugPrivilege");
            TokenManipulator.AddPrivilege("SeSystemEnvironmentPrivilege");
            Process.EnterDebugMode();
            Process process = Process.GetProcessesByName(nomJeu)[0];
            switch (nomJeu)
            {
                case "Xenotilt":
                    ProcessModule xenoModule = null;
                    foreach (ProcessModule m in process.Modules)
                    {
                        if (m.FileName.Contains("mono-2.0-bdwgc.dll"))
                        {
                            //Console.WriteLine("Adress :" + m.BaseAddress);
                            xenoModule = m;
                        }
                    }
                    processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);

                    int bytesRead = 0;
                    byte[] buffer = new byte[8];

                    if (xenoModule != null)
                    {

                        long adressToRead = xenoModule.BaseAddress.ToInt64() + 0x007270B8;
                        ReadProcessMemory((int)processHandle, adressToRead, buffer, buffer.Length, ref bytesRead);
                        //BitConverter.ToString(buffer);
                        Console.WriteLine(BitConverter.ToInt64(buffer, 0) +
                            " (" + bytesRead.ToString() + "bytes)");

                        adressToRead = BitConverter.ToInt64(buffer, 0) + 0x30;
                        ReadProcessMemory((int)processHandle, adressToRead, buffer, buffer.Length, ref bytesRead);
                        //BitConverter.ToString(buffer);
                        Console.WriteLine(BitConverter.ToInt64(buffer, 0) +
                            " (" + bytesRead.ToString() + "bytes)");

                        adressToRead = BitConverter.ToInt64(buffer, 0) + 0x7e0;
                        ReadProcessMemory((int)processHandle, adressToRead, buffer, buffer.Length, ref bytesRead);
                        //BitConverter.ToString(buffer);
                        Console.WriteLine(BitConverter.ToInt64(buffer, 0) +
                            " (" + bytesRead.ToString() + "bytes)");

                        adressToRead = BitConverter.ToInt64(buffer, 0) + 0x7C0;
                        scoreAdress = adressToRead;
                        _timer = new System.Timers.Timer { AutoReset = false, Interval = 1000 };
                        _timer.Elapsed += TimerElapsed;
                        _timer.AutoReset = true;
                        _timer.Enabled = true;
                    }
                    break;
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            getAndDisplayScore(processHandle, new byte[8], 0);
        }

        private void getAndDisplayScore(IntPtr processHandle, byte[] buffer, int bytesRead)
        {
            ReadProcessMemory((int)processHandle, scoreAdress, buffer, buffer.Length, ref bytesRead);

            score = BitConverter.ToInt64(buffer, 0);
            if (score > scoreObjectif * (nbHintsSent + 1))
            {
                nbHintsSent++;
                loginAndGetItem();
            }
            String scoreStr = score.ToString("#,#");
            this.Dispatcher.Invoke(() =>
            {
                scoreStr = score.ToString("#,#");
                scoreLabel.Content = scoreStr;
                progressBarScore.Value = Math.Floor(((score % scoreObjectif) / scoreObjectif) * 100);
                pourcentageScoreLabel.Content = progressBarScore.Value + " %";
            });

        }

        public void Main2()
        {




            loginAndGetItem();
            /*
            App.SingletonApp.session.Socket.SendPacket(new SayPacket
            {
                Text = "Connected"
            });
            */

        }

        private void loginAndGetItem()
        {
            /*
            if (archipelagoSession == null)
            {
                archipelagoSession = ArchipelagoSessionFactory.CreateSession("archipelago.gg", 51469);

                //App.SingletonApp.session = archipelagoSession;
                LoginResult loginResult;
                try
                {
                    Console.WriteLine("Start Connect & Login");
                    loginResult = archipelagoSession.TryConnectAndLogin("Doom 1993", "Boffbad", ItemsHandlingFlags.AllItems, new Version(0, 4, 0), null, null, null, true);
                }
                catch (Exception ex)
                {
                    loginResult = new LoginFailure(ex.GetBaseException().Message);
                }

                if (!loginResult.Successful)
                {
                    LoginFailure loginFailure = (LoginFailure)loginResult;
                    string text = "Failed to Connect ";
                    foreach (string str in loginFailure.Errors)
                    {
                        text = text + "\n    " + str;
                    }
                    foreach (ConnectionRefusedError connectionRefusedError in loginFailure.ErrorCodes)
                    {
                        text += string.Format("\n    {0}", connectionRefusedError);
                    }
                    Console.WriteLine(text);
                    return;
                }

                LoginSuccessful loginSuccessful = (LoginSuccessful)loginResult;
            }
            */
            List<long> copyMissingLocations = archipelagoSession.Locations.AllMissingLocations.ToList();

            Random rnd = new Random();
            int index = rnd.Next(copyMissingLocations.Count);
            locationsToRemove.Add(archipelagoSession.Locations.AllMissingLocations[index]);
            foreach (long l in locationsToRemove)
            {
                copyMissingLocations.Remove(l);
            }

            long locId = archipelagoSession.Locations.AllMissingLocations[index];
            this.Dispatcher.Invoke(() =>
            {
                logTextBox.Text += getItem(archipelagoSession, locId) + "\n";
            });
        }

        private static String getItem(ArchipelagoSession archipelagoSession, long locId)
        {
            Task<LocationInfoPacket> t = archipelagoSession.Locations.ScoutLocationsAsync(false, locId);
            //t.Start();
            //t.Wait();
            LocationInfoPacket x = t.Result;
            Console.WriteLine(archipelagoSession.Items.GetItemName(x.Locations[0].Item) + " is at " + archipelagoSession.Locations.GetLocationNameFromId(locId));

            return archipelagoSession.Items.GetItemName(x.Locations[0].Item) + " is at " + archipelagoSession.Locations.GetLocationNameFromId(locId);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Main2();
        }
    }
}
