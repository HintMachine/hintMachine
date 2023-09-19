namespace HintMachine.Games
{
    public class HintDetails : Archipelago.MultiClient.Net.Models.Hint
    {
        public string ReceivingPlayerName { get; set; }

        public string FindingPlayerName { get; set; }

        public string ItemName { get; set; }

        public string LocationName { get; set; }
    }
}
