namespace HintMachine
{
    public class HintQuestCumulative : HintQuestCounter
    {
        public enum CumulativeDirection
        {
            ASCENDING,
            DESCENDING
        }

        /// <summary>
        /// The direction of the tracking.
        /// If ASCENDING, value increments will be tracked and added to the quest progress.
        /// If DESCENDING, value decrements will be tracked instead.
        /// </summary>
        public CumulativeDirection Direction { get; set; } = CumulativeDirection.ASCENDING;

        private long _lastMemoryReading = long.MaxValue;

        // ----------------------------------------------------------------------------------

        public HintQuestCumulative()
        {}

        public void UpdateValue(long memoryReading)
        {
            if (Direction == CumulativeDirection.ASCENDING && memoryReading > _lastMemoryReading && (ThresholdValue != 0 && memoryReading - _lastMemoryReading < ThresholdValue))
                CurrentValue += (memoryReading - _lastMemoryReading);
            else if (Direction == CumulativeDirection.DESCENDING && memoryReading < _lastMemoryReading && (ThresholdValue != 0 && _lastMemoryReading - memoryReading < ThresholdValue))
                CurrentValue += (_lastMemoryReading - memoryReading);
            _lastMemoryReading = memoryReading;
        }
    }
}

