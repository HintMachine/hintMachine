using Archipelago.MultiClient.Net;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace HintMachine
{
    /// <summary>
    /// Logique d'interaction pour ManualHintWindow.xaml
    /// </summary>
    public partial class ManualHintWindow : Window
    {
        public delegate void ActionManualLocationHint(string locationName);
        public delegate void ActionManualItemHint(string itemName);

        public ActionManualLocationHint LocationHintCallback { get; set; }
        public ActionManualItemHint ItemHintCallback { get; set; }

        public ManualHintWindow(ArchipelagoHintSession archipelago)
        {
            InitializeComponent();

            foreach (string itemName in archipelago.GetItemNames())
                hintItemCombobox.Items.Add(itemName);
            foreach (string locName in archipelago.GetMissingLocationNames())
                hintLocationCombobox.Items.Add(locName);

            radioItem.IsChecked = true;

            if (hintLocationCombobox.Items.Count > 0)
                hintLocationCombobox.SelectedItem = hintLocationCombobox.Items[0];
            else
            {
                radioLocation.IsEnabled = false;
                hintLocationCombobox.IsEnabled = false;
                radioItem.IsChecked = true;
            }

            if (hintItemCombobox.Items.Count > 0)
                hintItemCombobox.SelectedItem = hintItemCombobox.Items[0];
            else
            {
                radioItem.IsEnabled = false;
                hintItemCombobox.IsEnabled = false;
                radioLocation.IsChecked = true;
            }

            if (!radioItem.IsEnabled && !radioLocation.IsEnabled)
                Close();
        }

        private void OnValidateButtonClick(object sender, RoutedEventArgs e)
        {
            if (radioItem.IsChecked == true && ItemHintCallback != null)
            {
                ItemHintCallback(hintItemCombobox.SelectedValue.ToString());
            }
            else if(radioLocation.IsChecked == true && LocationHintCallback != null)
            {
                LocationHintCallback(hintLocationCombobox.SelectedValue.ToString());
            }
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnComboboxChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = e.Source as ComboBox;
            if (comboBox == hintItemCombobox)
                radioItem.IsChecked = true;
            else
                radioLocation.IsChecked = true;
        }
    }
}
