
namespace RpgLibrary.DataClasses
{
    public class MinMaxPair
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 0;
        private Random Rnd { get; } = new();

        public MinMaxPair() { }
        public MinMaxPair(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int GetRandomNumberInRange()
        {
            return Rnd.Next(Min, Max + 1);
        }

        public override string ToString()
        {
            string toString = "Min = " + Min + "\n";
            toString += "Max = " + Max;
            return toString;
        }
    }
}
