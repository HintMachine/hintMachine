using HintMachine.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace HintMachine.Views
{
    /// <summary>
    /// Interaction logic for GameSelectionWindow.xaml
    /// </summary>
    public partial class GameSelectionWindow : Window
    {
        private readonly GameSelectionViewModel viewModel;

        public GameSelectionWindow()
        {
            InitializeComponent();

            viewModel = new GameSelectionViewModel();
            DataContext = viewModel;

            viewModel.CloseRequest += (sender, e) => Close();
        }

        private void ListGames_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.ListGames_MouseDoubleClick();
        }
    }
}
