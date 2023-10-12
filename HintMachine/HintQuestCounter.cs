using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HintMachine
{

    public class HintQuestCounter : HintQuest
    {
        /// <summary>
        /// The target value that must be reached 
        /// </summary>
        public long GoalValue { get; set; } = 1;

        /// <summary>
        /// If value moves by more than this value in one tick (cheats, memory values going nuts...), it's ignored
        /// </summary>
        public long MaxIncrease { get; set; } = 0;

        /// <summary>
        /// The current value reflecting current quest progression
        /// </summary>
        public long CurrentValue
        { 
            get 
            { 
                return _currentValue;
            } 
            set 
            {
                // If the TimeoutBetweenIncrements property is defined with a nonzero value, check the elapsed time
                // since last edit to cancel it if it seems to be cheating / malfunction
                if (TimeoutBetweenIncrements > 0 && value > _currentValue)
                {
                    DateTime now = DateTime.UtcNow;
                    TimeSpan t = now - _lastIncrementTime;
                    
                    if (t.TotalSeconds < TimeoutBetweenIncrements)
                        return;

                    _lastIncrementTime = now;
                }

                long absDiff = Math.Abs(_currentValue - value);
                if (MaxIncrease != 0 && absDiff > MaxIncrease && absDiff != GoalValue)
                    return;
                
                _currentValue = value;
            }
        }
        private long _currentValue = 0;
        private DateTime _lastIncrementTime = DateTime.MinValue;

        /// <summary>
        /// The time (in seconds) to wait before being able to increase the current value once more
        /// </summary>
        public int TimeoutBetweenIncrements { get; set; } = 0;

        private Label _label = null;
        private Label _labelDetail = null;
        private ProgressBar _progressBar = null;
        private TextBlock _progressBarOverlayText = null;

        // ----------------------------------------------------------------------------------

        public HintQuestCounter()
        {}
 
        public float GetProgression()
        {
            return (float)CurrentValue / GoalValue;
        }

        public override bool CheckCompletion()
        {
            if (CurrentValue < GoalValue)
                return false;

            CurrentValue -= GoalValue;
            return true;
        }

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
            _progressBarOverlayText.Text = CurrentValue + " / " + GoalValue;

#if DEBUG
            if (TimeoutBetweenIncrements > 0)
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan t = now - _lastIncrementTime;
                double cooldown = TimeoutBetweenIncrements - t.TotalSeconds;
                if(cooldown > 0)
                    _progressBarOverlayText.Text += $" ({Math.Ceiling(cooldown)}s cooldown)";
            }
#endif
        }
    }
}

