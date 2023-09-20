using Archipelago.MultiClient.Net;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace HintMachine
{
    public partial class HintsView : UserControl
    {
        public HintsView()
        {
            InitializeComponent();
        }

        public void UpdateItems(IEnumerable enumerable)
        {
            try
            {
                hintsList.ItemsSource = enumerable;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex.StackTrace);
            }
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
    }
}
