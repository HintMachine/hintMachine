namespace HintMachine
{
    internal class PlayerDetails
    {
        public string Game { get; set; }
        public string Name { get; set; }
        public bool GoalReached { get; set; }
    
        public int TotalChecks { get; set; }
        public int ChecksFound { get; set; }
    }
}