using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace XenotiltAP
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Please launch as administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }

            //fill combobox
            gameComboBox.Items.Add("Xenotilt");
            gameComboBox.SelectedIndex = 0;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //String[] adressAndPort = archipelagoAdress.Text.Split(':');
            //ArchipelagoSession archipelagoSession = ArchipelagoSessionFactory.CreateSession(adressAndPort[0], Int32.Parse(adressAndPort[1]));
            ArchipelagoSession archipelagoSession = ArchipelagoSessionFactory.CreateSession("archipelago.gg:52812");
            //App.SingletonApp.session = archipelagoSession;
            LoginResult loginResult;
            //RoomInfoPacket x = await archipelagoSession.ConnectAsync();
            try
            {
                Console.WriteLine("Start Connect & Login");
                //archipelagoSession.ConnectAsync();
                // PlayerInfo player = archipelagoSession.Players.AllPlayers.Where(p => p.Name.Equals(slotName.Text)).FirstOrDefault();
                String game;
                //if (player != null)
                //{
                //    game = player.Game;

                //}
                //else
                //{
                //    MessageBox.Show("Cant find player in slot.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //   return;

                //}
                game = "";
                loginResult = archipelagoSession.TryConnectAndLogin(game, slotName.Text, ItemsHandlingFlags.AllItems, new Version(0, 4, 2), new string[] { "AP", "TextOnly"}, null, null, true);
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
                MessageBox.Show("Connection error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            new MainWindow(archipelagoSession,gameComboBox.SelectedValue.ToString()).Show();
            this.Hide();
        }
    }
}
