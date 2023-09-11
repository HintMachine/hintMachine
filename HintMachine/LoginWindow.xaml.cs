using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;

namespace HintMachine
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            Settings.LoadFromFile();
            archipelagoAddress.Text = Settings.Host;
            slotName.Text = Settings.Slot;

            // Check that the app was launched with admin rights to be able to hook on any process
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Please launch as administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Connect to Archipelago
            ArchipelagoHintSession archipelagoSession = new ArchipelagoHintSession(archipelagoAddress.Text, slotName.Text);
            if (!archipelagoSession.isConnected)
            {
                MessageBox.Show("Could not connect to Archipelago: " + archipelagoSession.errorMessage, "Connection error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // If connectionn succeeded, store the fields contents for next execution and move on to MainWindow
            Settings.Host = archipelagoAddress.Text;
            Settings.Slot = slotName.Text;
            Settings.SaveToFile();

            new MainWindow(archipelagoSession).Show();
            Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Close the app when closing the window
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
