namespace EcosDelAzar.MiniGames
{
    public struct RoundResult
    {
        public RoundOutcome Outcome { get; }
        public int PlayerValue { get; }
        public int OpponentValue { get; }
        public string Description { get; }

        public RoundResult(RoundOutcome outcome, int playerValue, int opponentValue, string description = "")
        {
            Outcome = outcome;
            PlayerValue = playerValue;
            OpponentValue = opponentValue;
            Description = description;
        }
    }
}
