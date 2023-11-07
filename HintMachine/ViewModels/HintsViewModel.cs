using Archipelago.MultiClient.Net.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HintMachine.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace HintMachine.ViewModels
{
    public class HintsViewModel : ObservableObject
    {
        public ObservableCollection<HintDetails> HintList { get; set; }

        public ICollectionView HintListView => CollectionViewSource.GetDefaultView(HintList);          

        private bool progressionChecked;
        public bool ProgressionChecked
        {
            get => progressionChecked;
            set
            {
                if(SetProperty(ref progressionChecked, value))
                    HintListView.Refresh();
            }
        }

        public RelayCommand<string> SortColumn => new(SortColumnHandler);

        public HintsViewModel()
        {
            HintList = new ObservableCollection<HintDetails>();
            HintListView.Filter = new Predicate<object>(o => FilterHints(o as HintDetails));
        }

        public void UpdateHintList(List<HintDetails> hintList)
        {
            foreach (var hint in hintList.Where(hint => !HintList.Contains(hint)))
            {
                HintList.Add(hint);
            }
        }

        private bool FilterHints(HintDetails hint)
        {
            if (hint.Found)
                return false;

            // Filter out non-progression items if related checkbox is checked
            if (ProgressionChecked && (!hint.ItemFlags.HasFlag(ItemFlags.Advancement)))
                return false;

            return true;
        }

        private void SortColumnHandler(string propertyName)
        {
            var direction = ListSortDirection.Ascending;

            var sortDescription = HintListView.SortDescriptions.FirstOrDefault(d => d.PropertyName == propertyName);
            if (sortDescription.Direction == ListSortDirection.Ascending)
            {
                direction = ListSortDirection.Descending;
            }

            HintListView.SortDescriptions.Clear();
            HintListView.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }
    }
}
