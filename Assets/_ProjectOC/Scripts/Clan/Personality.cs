using Sirenix.OdinInspector;

namespace ProjectOC.ClanNS
{
    [LabelText("ÐÔ¸ñ"), System.Serializable]
    public struct Personality
    {
        public int Value;
        public int Low;
        public int High;
        public Personality(int value, int low, int high)
        {
            Value = value;
            Low = low;
            High = high;
        }
        public Personality((int, int, int) tuple)
        {
            Value = tuple.Item1;
            Low = tuple.Item2;
            High = tuple.Item3;
        }
        public Personality ChangeValue(int value)
        {
            value += Value;
            value = System.Math.Min(value, Low);
            value = System.Math.Max(value, High);
            Value = value;
            return this;
        }
    }
}