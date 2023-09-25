using HintMachine.Games;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Archipelago.MultiClient.Net.Enums;

namespace HintMachine
{
    public partial class HintsView : UserControl
    {
        private List<HintDetails> _fullHintsList = null;

        public HintsView()
        {
            InitializeComponent();
        }

        public void UpdateItems(List<HintDetails> knownHints)
        {
            _fullHintsList = knownHints;

            List<HintDetails> filteredHints = new List<HintDetails>();
            foreach (HintDetails hint in knownHints)
            {
                // Filter out already found items
                if (hint.Found)
                    continue;
                // Filter out non-progression items if related checkbox is checked
                if (checkboxProgression.IsChecked == true && !hint.ItemFlags.HasFlag(ItemFlags.Advancement))
                    continue;

                filteredHints.Add(hint);
            }
            
            Dispatcher.Invoke(() =>
            {
                hintsList.ItemsSource = filteredHints;
            });
        }

        private void OnHintsListColumnClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();

            ListSortDirection direction = ListSortDirection.Ascending;
            foreach (SortDescription desc in hintsList.Items.SortDescriptions)
            {
                if (desc.PropertyName == sortBy)
                {
                    direction = (desc.Direction == ListSortDirection.Ascending)
                                ? ListSortDirection.Descending
                                : ListSortDirection.Ascending;
                    break;
                }
            }

            hintsList.Items.SortDescriptions.Clear();
            hintsList.Items.SortDescriptions.Add(new SortDescription(sortBy, direction));
        }

        private void OnCheckboxProgressionChecked(object sender, RoutedEventArgs e)
        {
            if(hintsList != null && _fullHintsList != null)
                UpdateItems(_fullHintsList);
        }
    }
}
