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
            viewModel.UpdatePropertiesRequest += UpdateSelectedGameProperties;

            UpdateSelectedGameProperties(this, EventArgs.Empty);
        }

        private void UpdateSelectedGameProperties(object sender, EventArgs e)
        {
            if (viewModel.SelectedGame == null)
                return;

            TextGameProperties.Text = string.Empty;

            // Supported versions
            if (viewModel.SelectedGame.SupportedVersions.Any())
            {
                if (viewModel.SelectedGame.SupportedVersions.Count == 1)
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported version: ")));
                    TextGameProperties.Inlines.Add(viewModel.SelectedGame.SupportedVersions[0]);
                }
                else
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported versions: ")));
                    foreach (string version in viewModel.SelectedGame.SupportedVersions)
                    {
                        TextGameProperties.Inlines.Add(new LineBreak());
                        TextGameProperties.Inlines.Add($"    • {version}");
                    }
                }

                TextGameProperties.Inlines.Add(new LineBreak());
                TextGameProperties.Inlines.Add(new LineBreak());
            }

            // Supported emulators
            if (viewModel.SelectedGame.SupportedEmulators.Any())
            {
                if (viewModel.SelectedGame.SupportedEmulators.Count == 1)
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported emulator: ")));
                    TextGameProperties.Inlines.Add(viewModel.SelectedGame.SupportedEmulators[0]);
                }
                else
                {
                    TextGameProperties.Inlines.Add(new Bold(new Run("Supported emulators: ")));
                    foreach (string emulator in viewModel.SelectedGame.SupportedEmulators)
                    {
                        TextGameProperties.Inlines.Add(new LineBreak());
                        TextGameProperties.Inlines.Add($"  • {emulator}");
                    }
                }
                TextGameProperties.Inlines.Add(new LineBreak());
                TextGameProperties.Inlines.Add(new LineBreak());
            }

            // Quests
            TextGameProperties.Inlines.Add(new Bold(new Run("Quests: ")));

            string questsString = string.Empty;
            foreach (var quest in viewModel.SelectedGame.Quests)
            {
                if (!string.IsNullOrEmpty(questsString))
                    questsString += ", ";
                questsString += quest.Name;
            }
            TextGameProperties.Inlines.Add(questsString);

            TextGameProperties.Inlines.Add(new LineBreak());
            TextGameProperties.Inlines.Add(new LineBreak());

            // Author
            TextGameProperties.Inlines.Add(new Bold(new Run("Author: ")));
            TextGameProperties.Inlines.Add(viewModel.SelectedGame.Author);
        }

        private void ListGames_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            viewModel.ListGames_MouseDoubleClick();
        }
    }
}
