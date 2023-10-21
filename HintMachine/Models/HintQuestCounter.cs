using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HintMachine.Models
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
                if (CooldownBetweenIncrements > 0 && value > _currentValue)
                {
                    DateTime now = DateTime.UtcNow;
                    TimeSpan t = now - _lastIncrementTime;

                    if (t.TotalSeconds < CooldownBetweenIncrements)
                    {
                        Logger.Debug($"Quest '{Name}' tried to increase faster than CooldownBetweenIncrements, denied increase.");
                        return;
                    }

                    _lastIncrementTime = now;
                }

                // If value is increasing, check if it does not increase more than the defined MaxIncrease
                // (this is mostly used to prevent blatant cheating / bugged value readings)
                if(value > _currentValue)
                {
                    long diff = value - _currentValue;

                    // If MaxIncrease is not defined for that quest, use a default value of (2 x GoalValue)
                    long realMaxIncrease = (MaxIncrease > 0) ? MaxIncrease : GoalValue * 2;

                    if (diff > realMaxIncrease)
                    {
                        Logger.Debug($"Quest '{Name}' tried to increase more than MaxIncrease, denied increase.");
                        return;
                    }
                }
                
                _currentValue = value;
            }
        }
        private long _currentValue = 0;
        private DateTime _lastIncrementTime = DateTime.MinValue;

        /// <summary>
        /// The time (in seconds) to wait before being able to increase the current value once more
        /// </summary>
        public int CooldownBetweenIncrements { get; set; } = 0;

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

        public override int CheckAndCommitCompletion()
        {
            int obtainedHints = 0;
            
            if (GoalValue == 0)
                return 0;

            while (CurrentValue >= GoalValue)
            {
                obtainedHints += AwardedHints;
                CurrentValue -= GoalValue;
            }

            return obtainedHints;
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
            if (CooldownBetweenIncrements > 0)
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan t = now - _lastIncrementTime;
                double cooldown = CooldownBetweenIncrements - t.TotalSeconds;
                if(cooldown > 0)
                    _progressBarOverlayText.Text += $" ({Math.Ceiling(cooldown)}s cooldown)";
            }
#endif
        }
    }
}

