using System.Windows;
using System.Windows.Controls;

namespace HintMachine
{ 
    public class HintQuest
    {
        public string displayName;
        public long goalValue;
        public long currentValue = 0;
        public string questType;
        public bool hasBeenAwarded = false;
        public int numberOfHintsGiven = 1;

        Label _label = null;
        ProgressBar _progressBar = null;
        TextBlock _progressBarOverlayText = null;

        public HintQuest(string displayName, long goalValue, string questType = "cumulative", int numberOfHintsGiven = 1)
        {
            this.displayName = displayName;
            this.goalValue = goalValue;
            this.questType = questType;
            this.numberOfHintsGiven = numberOfHintsGiven;
        }

        public void Add(long increment)
        {
            currentValue += increment;
        }

        public void SetValue(long value)
        {
            currentValue = value;
        }

        public float GetProgression()
        {
            return (float)currentValue / goalValue;
        }

        public bool CheckCompletion()
        {
            if (currentValue < goalValue)
                return false;

            if (currentValue == goalValue && questType.Equals("objective"))
            {
                if (hasBeenAwarded)
                {
                    return false;
                }
                else
                {
                    hasBeenAwarded = true;
                    return true;
                }
            }
            else { 
                currentValue -= goalValue;
                return true;
            }
        }

        public void InitComponents(Grid questsGrid)
        {
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(30);
            questsGrid.RowDefinitions.Add(rowDef);

            // Add a label
            _label = new Label();
            _label.Content = displayName;
            _label.Margin = new Thickness(0, 0, 8, 4);
            _label.FontWeight = FontWeights.Bold;
            Grid.SetColumn(_label, 0);
            Grid.SetRow(_label, questsGrid.RowDefinitions.Count-1);
            questsGrid.Children.Add(_label);

            // Add a grid containing overlapped ProgressBar + TextBlock to have an overlay text over the progressbar
            _progressBar = new ProgressBar();
            _progressBar.Minimum = 0;
            _progressBar.Maximum = goalValue;

            _progressBarOverlayText = new TextBlock();
            _progressBarOverlayText.HorizontalAlignment = HorizontalAlignment.Center;
            _progressBarOverlayText.VerticalAlignment = VerticalAlignment.Center;

            Grid progressBarGrid = new Grid();
            progressBarGrid.Margin = new Thickness(0, 0, 0, 4);
            progressBarGrid.Children.Add(_progressBar);
            progressBarGrid.Children.Add(_progressBarOverlayText);
            Grid.SetColumn(progressBarGrid, 1);
            Grid.SetRow(progressBarGrid, questsGrid.RowDefinitions.Count - 1);
            questsGrid.Children.Add(progressBarGrid);
        }

        public void UpdateComponents()
        {
            _progressBar.Value = currentValue;
            _progressBarOverlayText.Text = currentValue + " / " + goalValue;
        }
    }
}

