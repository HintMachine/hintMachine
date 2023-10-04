using HintMachine.Games;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Archipelago.MultiClient.Net.Enums;
using System;
using System.Linq;

namespace HintMachine
{
    public partial class PlayersView : UserControl
    {
        private List<PlayerDetails> _fullPlayerList = null;
        // ----------------------------------------------------------------------------------

        public PlayersView()
        {
            InitializeComponent();
            _fullPlayerList = new List<PlayerDetails>();
            /*
            foreach (string playerName in _archipelagoSession.GetPlayerNames())
            {
                if (playerName == "Server")
                    continue;
            }
            */
        }

        public void UpdatePlayers(ArchipelagoHintSession archipelagoHintSession)
        {
            _fullPlayerList.Clear();
            PlayerDetails pDet;
            foreach (string playerName in archipelagoHintSession.GetPlayerNames())
            {
                if (playerName == "Server")
                    continue;

                pDet = new PlayerDetails();
                pDet.Name = playerName;
                pDet.Game = archipelagoHintSession.Client.Players.AllPlayers.First(x => x.Name == playerName)?.Game;
                if (playerName == archipelagoHintSession.Slot)
                {
                    pDet.ChecksFound = archipelagoHintSession.Client.Locations.AllLocationsChecked.Count;
                    pDet.TotalChecks = archipelagoHintSession.Client.Locations.AllLocations.Count;
                }
                else
                {
                    //afficher le nb de checks pour chaque joueur
                }
                pDet.GoalReached = false;
                
                _fullPlayerList.Add(pDet);
            }
            ListViewPlayers.ItemsSource = _fullPlayerList;

            // Adjust all columns' size to fit their contents, as if the column header was double-clicked
            foreach (GridViewColumn c in gridPlayers.Columns)
            {
                // Code below was found in GridViewColumnHeader.OnGripperDoubleClicked() event handler (using Reflector)
                if (double.IsNaN(c.Width))
                {
                    c.Width = c.ActualWidth;
                }
                c.Width = double.NaN;
            }
        }
        /*
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
       if ((CheckboxProgression.IsChecked ?? true) && (!hint.ItemFlags.HasFlag(ItemFlags.Advancement)))
           continue;

       filteredHints.Add(hint);
   }

   ListViewHints.ItemsSource = filteredHints;

   // Adjust all columns' size to fit their contents, as if the column header was double-clicked
   foreach (GridViewColumn c in grid.Columns)
   {
       // Code below was found in GridViewColumnHeader.OnGripperDoubleClicked() event handler (using Reflector)
       if (double.IsNaN(c.Width))
       {
           c.Width = c.ActualWidth;
       }
       c.Width = double.NaN;
   }
}

private void OnHintsListColumnClick(object sender, RoutedEventArgs e)
{
   GridViewColumnHeader column = (sender as GridViewColumnHeader);
   string sortBy = column.Tag.ToString();

   ListSortDirection direction = ListSortDirection.Ascending;
   foreach (SortDescription desc in ListViewHints.Items.SortDescriptions)
   {
       if (desc.PropertyName == sortBy)
       {
           direction = (desc.Direction == ListSortDirection.Ascending)
                       ? ListSortDirection.Descending
                       : ListSortDirection.Ascending;
           break;
       }
   }

   ListViewHints.Items.SortDescriptions.Clear();
   ListViewHints.Items.SortDescriptions.Add(new SortDescription(sortBy, direction));
}

private void OnCheckboxProgressionChecked(object sender, RoutedEventArgs e)
{
   if (ListViewHints != null && _fullHintsList != null)
       UpdateItems(_fullHintsList);
}
*/
    }
}
