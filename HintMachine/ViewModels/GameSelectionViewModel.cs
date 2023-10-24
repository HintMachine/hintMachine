using CommunityToolkit.Mvvm.Input;
using HintMachine.Models;
using HintMachine.Models.GenericConnectors;
using HintMachine.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace HintMachine.ViewModels
{
    internal class GameSelectionViewModel : ICloseableViewModel {
        public delegate void OnGameConnectedAction(IGameConnector game);
        public event OnGameConnectedAction OnGameConnected = null;

        public List<IGameConnector> GameList { get; set; }

        public ICollectionView GameListView => CollectionViewSource.GetDefaultView(GameList);

        private IGameConnector selectedGame;
        public IGameConnector SelectedGame
        {
            get => selectedGame;
            set
            {
                SetProperty(ref selectedGame, value);
            }
        }

        private string searchText;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (SetProperty(ref searchText, value))
                {
                    var previouslySelectedGame = SelectedGame;
                    GameListView?.Refresh();
                    SelectedGame = previouslySelectedGame;
                }
            }
        }

        public RelayCommand ClearSearchText => new RelayCommand(ClearSearchTextHandler);

        public RelayCommand Validate => new RelayCommand(ValidateHandler);

        public RelayCommand Cancel => new RelayCommand(CancelHandler);

        public GameSelectionViewModel()
        {
            GameList = Globals.Games.OrderBy(g => g.Name).ToList();

            SelectedGame = Globals.FindGameFromName(Settings.LastConnectedGame);
            if (SelectedGame == null)
                SelectedGame = GameList.FirstOrDefault();

            GameListView.Filter = new Predicate<object>(o => FilterGames(o as IGameConnector));
        }

        private bool FilterGames(IGameConnector game)
        {
            if (string.IsNullOrEmpty(SearchText))
                return true;

            foreach (string word in SearchText.Trim().ToLower().Split(' '))
            {
                if (game.Name.ToLower().Contains(word))
                    continue;

                if (game.Platform.ToLower().Contains(word))
                    continue;

                return false;
            }

            return true;
        }

        private void ClearSearchTextHandler()
        {
            SearchText = string.Empty;
        }

        public void ListGames_MouseDoubleClick()
        {
            if (SelectedGame != null)
                ValidateHandler();
        }

        private void ValidateHandler()
        {
            // Connect to selected game
            if (!SelectedGame.DoConnect())
            {
                string message = $"Could not connect to {SelectedGame.Name}.{Environment.NewLine}" +
                                  "Please ensure it is currently running and try again.";
                MessageBox.Show(message, "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OnGameConnected?.Invoke(SelectedGame);

            CloseWindow();
        }

        private void CancelHandler()
        {
            CloseWindow();
        }
    }
}
