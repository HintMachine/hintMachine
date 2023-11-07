using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HintMachine.Models
{
    internal class HintQuestSingle : HintQuestCounter
    {
        public bool Completed { get; set; } = false;

        public override int CheckAndCommitCompletion()
        {
            int obtainedHints = 0;

            if (GoalValue == 0)
                return 0;

            if (CurrentValue >= GoalValue && !Completed)
            {
                obtainedHints += AwardedHints;
                Completed = true;
            }

            return obtainedHints;
        }

        // This code might become relevant again as I research the new configuration
        // and determine if I can recreate the functionality in the new system
        /*
        private Label _label = null;
        private Label _labelDetail = null;
        private ProgressBar _progressBar = null;
        private TextBlock _progressBarOverlayText = null;
        private DateTime _lastIncrementTime = DateTime.MinValue;

        public override void InitComponents(Grid questsGrid)
        {
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(30);
            questsGrid.RowDefinitions.Add(rowDef);

            // Add a label for the quest name
            _label = new Label
            {
                Content = Name,
                Margin = new Thickness(0, 0, 8, 4),
                FontWeight = FontWeights.Bold,
            };
            Grid.SetColumn(_label, 0);
            Grid.SetRow(_label, questsGrid.RowDefinitions.Count - 1);
            questsGrid.Children.Add(_label);

            // If there is a detailed quest description available, add a question mark with this detailed
            // description as a tooltip
            if (Description.Trim().Length != 0)
            {
                _labelDetail = new Label
                {
                    Content = "(?)",
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 150, 200)),
                    Margin = new Thickness(-4, 0, 8, 4),
                    FontWeight = FontWeights.Bold,
                    ToolTip = Description,
                };

                Grid.SetColumn(_labelDetail, 1);
                Grid.SetRow(_labelDetail, questsGrid.RowDefinitions.Count - 1);
                questsGrid.Children.Add(_labelDetail);
            }

            // Add a grid containing overlapped ProgressBar + TextBlock to have an overlay text over the progressbar
            _progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = GoalValue,
            };
            _progressBarOverlayText = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            Grid progressBarGrid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            progressBarGrid.Children.Add(_progressBar);
            progressBarGrid.Children.Add(_progressBarOverlayText);
            Grid.SetColumn(progressBarGrid, 2);
            Grid.SetRow(progressBarGrid, questsGrid.RowDefinitions.Count - 1);
            questsGrid.Children.Add(progressBarGrid);
        }

        public override void UpdateComponents()
        {
            _progressBar.Value = CurrentValue;
            if (Completed) _progressBarOverlayText.Text = "Completed";
            else _progressBarOverlayText.Text = CurrentValue + " / " + GoalValue;

#if DEBUG
            if (CooldownBetweenIncrements > 0)
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan t = now - _lastIncrementTime;
                double cooldown = CooldownBetweenIncrements - t.TotalSeconds;
                if (cooldown > 0)
                    _progressBarOverlayText.Text += $" ({Math.Ceiling(cooldown)}s cooldown)";
            }
#endif
        }
        */
    }
}