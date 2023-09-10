using System.Collections;
using System.ComponentModel;
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

            // Check that the app was launched with admin rights to be able to hook on any process
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Please launch as administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }


            // Fill game selector combobox with supported game names

            GamesList.GAMES.Sort((a,b) => a.GetDisplayName().CompareTo(b.GetDisplayName()));
            foreach (IGameConnector game in GamesList.GAMES)
            {
                gameComboBox.Items.Add(game.GetDisplayName());
            }

            gameComboBox.SelectedIndex = 0;
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
            
            // Connect to selected game
            string selectedGameName = gameComboBox.SelectedValue.ToString();
            IGameConnector game = GamesList.FindGameFromName(selectedGameName);
     
            if(!game.Connect())
            {
                MessageBox.Show("Could not connect to the selected game. Please ensure it is currently running.", "Error",
                                 MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // If both connections succeeded, move on to MainWindow
            new MainWindow(archipelagoSession, game).Show();
            Hide();
        }
    }
}
