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
    public class GameSelectionViewModel : ICloseableViewModel
    {
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
            GameList = HintMachineService.GameConnectorTypes.Select(type => Activator.CreateInstance(type) as IGameConnector)
                                                            .OrderBy(game => game.Name)
                                                            .ToList();
            foreach (IGameConnector game in GameList)
            {
                if (game.Name == Settings.LastConnectedGame)
                {
                    SelectedGame = game;
                    break;
                }
            }
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
            if (HintMachineService.ConnectToGame(SelectedGame))
            {
                // Connection was a success, just close this window
                CloseWindow();
            }
            else
            {
                // Connection failed for some reason, display an error message inside a MessageBox
                MessageBox.Show($"Could not connect to {SelectedGame.Name}.\nPlease ensure it is currently running and try again.", 
                    "Connection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelHandler()
        {
            CloseWindow();
        }
    }
}
