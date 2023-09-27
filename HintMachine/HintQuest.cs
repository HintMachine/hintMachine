using System.Windows;
using System.Windows.Controls;

namespace HintMachine
{
    public abstract class HintQuest
    {
        /// <summary>
        /// The name of the quest, as shown to the user in the quests list
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// A detailed description of what must be accomplished in the quest in order to get hints.
        /// If not blank, a convenient "question mark" widget will be added with this string as a tooltip.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The number of hints awarded on quest completion
        /// </summary>
        public int AwardedHints { get; set; } = 1;

        // ----------------------------------------------------------------------------------

        public HintQuest()
        {}

        public abstract bool CheckCompletion();

        public abstract void InitComponents(Grid questsGrid);

        public abstract void UpdateComponents();
    }
}

