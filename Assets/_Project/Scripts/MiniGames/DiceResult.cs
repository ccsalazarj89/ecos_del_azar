namespace EcosDelAzar.MiniGames
{
    public readonly struct DiceResult
    {
        public int Value { get; }
        public DiceResult(int value) => Value = value;
    }
}
