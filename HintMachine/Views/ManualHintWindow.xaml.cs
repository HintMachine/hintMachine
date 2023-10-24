using HintMachine.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HintMachine.Views
{
    /// <summary>
    /// Logique d'interaction pour ManualHintWindow.xaml
    /// </summary>
    public partial class ManualHintWindow : Window
    {
        public Action<String> HintLocationCallback { get; set; }
        public Action<String> HintItemCallback { get; set; }

        // ----------------------------------------------------------------------------------

        public ManualHintWindow(ArchipelagoHintSession archipelago)
        {
            InitializeComponent();

            foreach (string itemName in archipelago.GetItemNames())
                ComboboxHintedItem.Items.Add(itemName);
            foreach (string locName in archipelago.GetMissingLocationNames())
                ComboboxHintedLocation.Items.Add(locName);

            RadioHintItem.IsChecked = true;

            if (ComboboxHintedLocation.Items.Count > 0)
                ComboboxHintedLocation.SelectedItem = ComboboxHintedLocation.Items[0];
            else
            {
                RadioHintLocation.IsEnabled = false;
                ComboboxHintedLocation.IsEnabled = false;
                RadioHintItem.IsChecked = true;
            }

            if (ComboboxHintedItem.Items.Count > 0)
                ComboboxHintedItem.SelectedItem = ComboboxHintedItem.Items[0];
            else
            {
                RadioHintItem.IsEnabled = false;
                ComboboxHintedItem.IsEnabled = false;
                RadioHintLocation.IsChecked = true;
            }

            if (!RadioHintItem.IsEnabled && !RadioHintLocation.IsEnabled)
                Close();
        }

        private void OnValidateButtonClick(object sender, RoutedEventArgs e)
        {
            if (RadioHintItem.IsChecked == true)
            {
                HintItemCallback?.Invoke(ComboboxHintedItem.SelectedValue.ToString());
            }
            else if(RadioHintLocation.IsChecked == true)
            {
                HintLocationCallback?.Invoke(ComboboxHintedLocation.SelectedValue.ToString());
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
            if (comboBox == ComboboxHintedItem)
                RadioHintItem.IsChecked = true;
            else
                RadioHintLocation.IsChecked = true;
        }
    }
}
