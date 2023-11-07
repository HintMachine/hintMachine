using System;
using System.ComponentModel;
using System.Windows.Controls;
using HintMachine.Helpers;
using HintMachine.Views;

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
                NotifyPropertyChanged(nameof(CurrentValue));
            }
        }
        private long _currentValue = 0;
        private DateTime _lastIncrementTime = DateTime.MinValue;

        /// <summary>
        /// The time (in seconds) to wait before being able to increase the current value once more
        /// </summary>
        public int CooldownBetweenIncrements { get; set; } = 0;

        private QuestControl _control = null;

        // ----------------------------------------------------------------------------------

        public HintQuestCounter()
        {}

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

        public override void InitComponents(StackPanel questsPanel)
        {
            _control = new QuestControl { DataContext = this };
            questsPanel.Children.Add(_control);
        }

        /*
        public override void UpdateComponents()
        {
            if (HintMachineService.DebugBuild && CooldownBetweenIncrements > 0)
            {
                DateTime now = DateTime.UtcNow;
                TimeSpan t = now - _lastIncrementTime;
                double cooldown = CooldownBetweenIncrements - t.TotalSeconds;
                if(cooldown > 0)
                    _progressBarOverlayText.Text += $" ({Math.Ceiling(cooldown)}s cooldown)";
            }
        }
        */
    }
}

