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
            gameComboBox.Items.Add("One Finger Death Punch");
            gameComboBox.Items.Add("Xenotilt");

            gameComboBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            ArchipelagoSession archipelagoSession = ArchipelagoSessionFactory.CreateSession(archipelagoAdress.Text);
            LoginResult loginResult;
            try
            {
                Console.WriteLine("Start Connect & Login");

                String game;

                game = "";
                loginResult = archipelagoSession.TryConnectAndLogin(game, slotName.Text, ItemsHandlingFlags.AllItems, new Version(0, 4, 1), new string[] { "AP", "TextOnly"}, null, null, true);
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
