namespace EcosDelAzar.MiniGames
{
    public sealed class DiceWinnerEvaluator
    {
        public DiceWinner Evaluate(DiceResult playerOne, DiceResult playerTwo)
        {
            if (playerOne.Value > playerTwo.Value) return DiceWinner.PlayerOne;
            if (playerTwo.Value > playerOne.Value) return DiceWinner.PlayerTwo;
            return DiceWinner.Draw;
        }
    }
}
