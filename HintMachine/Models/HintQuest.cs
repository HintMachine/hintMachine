using System.ComponentModel;
using System.Windows.Controls;

namespace HintMachine.Models
{
    public abstract class HintQuest : INotifyPropertyChanged
    {
        /// <summary>
        /// The name of the quest, as shown to the user in the quests list
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { 
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }
        private string _name = string.Empty;

        /// <summary>
        /// A detailed description of what must be accomplished in the quest in order to get hints.
        /// If not blank, a convenient "question mark" widget will be added with this string as a tooltip.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The number of hints awarded on quest completion
        /// </summary>
        public int AwardedHints { get; set; } = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        // ----------------------------------------------------------------------------------

        public HintQuest()
        {}

        /// <summary>
        /// A method which checks if quest has been completed, decreases the CurrentValue until the quest reaches a
        /// "non-completed" state and returns the number of awarded hints.
        /// </summary>
        /// <returns>the number of obtained hints</returns>
        public abstract int CheckAndCommitCompletion();

        public abstract void InitComponents(StackPanel questsPanel);

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

