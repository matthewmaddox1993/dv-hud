namespace QuantitiesNet
{
    public readonly struct Prefix
    {
        public readonly string symbol;
        public readonly double scalar;

        public Prefix(string symbol, int powerOfTen)
        {
            this.symbol = symbol;
            this.scalar = (double)System.Math.Pow(10, powerOfTen);
        }
    }

    public static class Prefixes
    {
        public static readonly Prefix Yotta = new Prefix("Y", 24);
        public static readonly Prefix Zetta = new Prefix("Z", 21);
        public static readonly Prefix Exa = new Prefix("E", 18);
        public static readonly Prefix Peta = new Prefix("P", 15);
        public static readonly Prefix Tera = new Prefix("T", 12);
        public static readonly Prefix Giga = new Prefix("G", 9);
        public static readonly Prefix Mega = new Prefix("M", 6);
        public static readonly Prefix Kilo = new Prefix("k", 3);
        public static readonly Prefix Hecto = new Prefix("h", 2);
        public static readonly Prefix Deca = new Prefix("d", 1);
        public static readonly Prefix Deci = new Prefix("d", -1);
        public static readonly Prefix Centi = new Prefix("c", -2);
        public static readonly Prefix Milli = new Prefix("m", -3);
        public static readonly Prefix Micro = new Prefix("Î¼", -6);
        public static readonly Prefix Nano = new Prefix("n", -9);
        public static readonly Prefix Pico = new Prefix("p", -12);
        public static readonly Prefix Femto = new Prefix("f", -15);
        public static readonly Prefix Atto = new Prefix("a", -18);
        public static readonly Prefix Zepto = new Prefix("z", -21);
        public static readonly Prefix Yocto = new Prefix("y", -24);
    }
}


