namespace GenAIExpertEngineAPI.Classes
{
    public static class Dice
    {
        public static readonly Random random = new Random();
        private const int D4 = 4;
        private const int D6 = 6;
        private const int D8 = 8;
        private const int D10 = 10;
        private const int D12 = 12;
        private const int D20 = 20;
        private const int D100 = 100;

        private static int RollD4() => random.Next(1, D4 + 1);
        private static int RollD6() => random.Next(1, D6 + 1);
        private static int RollD8() => random.Next(1, D8 + 1);
        private static int RollD10() => random.Next(1, D10 + 1);
        private static int RollD12() => random.Next(1, D12 + 1);
        private static int RollD20() => random.Next(1, D20 + 1);
        private static int RollD100() => random.Next(1, D100 + 1);

        public static int RollDie(DiceType die)
        {
            return die switch
            {
                DiceType.D4 => RollD4(),
                DiceType.D6 => RollD6(),
                DiceType.D8 => RollD8(),
                DiceType.D10 => RollD10(),
                DiceType.D12 => RollD12(),
                DiceType.D20 => RollD20(),
                DiceType.D100 => RollD100(),
                _ => throw new ArgumentException("Invalid die type"),
            };
        }

        public static DiceType ConvertIntToDice(int value)
        {
            switch (value)
            {
                case 4: return DiceType.D4;
                case 6: return DiceType.D6;
                case 8: return DiceType.D8;
                case 10: return DiceType.D10;
                case 12: return DiceType.D12;
                case 20: return DiceType.D20;
                case 100: return DiceType.D100;
                default: throw new ArgumentException("Invalid die value");
            }
        }

        public static int RollDice(DiceType diceType, int numberOfDice)
        {
            int total = 0;
            for (int i = 0; i < numberOfDice; i++)
            {
                total += RollDie(diceType);
            }
            return total;
        }

        public static int RollDice(List<DiceType> damage)
        {
            int total = 0;
            foreach (DiceType die in damage)
            {
                total += RollDie(die);
            }
            return total;
        }
    }
    public enum DiceType
    {
        D4,
        D6,
        D8,
        D10,
        D12,
        D20,
        D100
    }
}
