using System;
using static QuantitiesNet.Dimensions;
using static QuantitiesNet.Prefixes;

namespace QuantitiesNet
{
    public class Unit : IDimension
    {
        public string Symbol { get; }
        public Dimension Dimension { get; }
        // <summary>Number of Dimension base units represented by one of this Unit.</summary>
        public double Scalar { get; }
        // <summary>Zero point of this unit in Dimension base units.</summary>
        public double Offset { get; }

        public Unit(string symbol, Dimension dim, double scalar, double offset = 0)
        {
            this.Symbol = symbol;
            this.Dimension = dim;
            this.Scalar = scalar;
            this.Offset = offset;
        }

        public static Unit Of<D>(string symbol, double scalar, double offset = 0)
        where D : IDimension, new()
        {
            return new Unit(
                symbol,
                Dimension.ForType<D>(),
                scalar,
                offset);
        }

        public static Unit Of<D>(string symbol, double scalar, Unit basis)
        where D : IDimension, new()
        {
            if (basis.Dimension != Dimension.ForType<D>())
                throw new ArgumentException($"basis dimension {basis.Dimension} does not match expected dimension {typeof(D).Name}");
            if (basis.Offset != 0)
                throw new ArgumentException("Cannot scale unit with offset");
            return new Unit(
                symbol,
                basis.Dimension,
                scalar * basis.Scalar,
                0);
        }

        public override string ToString()
        {
            return $"{Symbol} ({Dimension})";
        }

        public static Unit operator * (Prefix p, Unit baseUnit)
        {
            return new Unit(p.symbol + baseUnit.Symbol, baseUnit.Dimension, p.scalar * baseUnit.Scalar);
        }

        public const char DotOperator = '\u22C5';

        public static Unit operator * (Unit u1, Unit u2)
        {
            return new Unit($"{u1.Symbol}\u22C5{u2.Symbol}", u1.Dimension * u2.Dimension, u1.Scalar * u2.Scalar);
        }

        public static Unit operator / (Unit u1, Unit u2)
        {
            return new Unit($"{u1.Symbol}/{u2.Symbol}", u1.Dimension / u2.Dimension, u1.Scalar / u2.Scalar);
        }
    }

    public class Unit<D> : Unit
        where D : IDimension, new()
    {
        public Unit(string symbol, double scalar, double offset = 0)
        : base(symbol, Dimension.ForType<D>(), scalar, offset)
        {
        }

        public Unit(string symbol, double scalar, Unit basis)
        : base(symbol, Dimension.ForType<D>(), scalar * basis.Scalar)
        {
            if (basis.Dimension != Dimension.ForType<D>())
                throw new ArgumentException($"basis dimension {basis.Dimension} does not match expected dimension {typeof(D).Name}");
        }

        public static Unit<D> operator * (Prefix p, Unit<D> baseUnit)
        {
            return new Unit<D>(p.symbol + baseUnit.Symbol, p.scalar * baseUnit.Scalar);
        }

        public static Quantity<D> operator * (double scalar, Unit<D> unit)
        {
            return new Quantity<D>(scalar, unit);
        }
    }

    public static class UnitExtensions
    {
        public static Unit<D> Assert<D>(this Unit unit)
            where D : IDimension, new()
        {
            if (unit.Dimension != Dimension.ForType<D>())
                throw new ArgumentException($"basis dimension {unit.Dimension} does not match expected dimension {typeof(D).Name}");
            return new Unit<D>(unit.Symbol, unit.Scalar, unit.Offset);
        }
    }

    public static class Units
    {
        // static constructor ensures that all static fields are initialized before Initialize() completes.
        static Units() { }
        public static void Initialize() { }

        private static Unit<D> Register<D>(Unit<D> unit)
            where D : IDimension, new()
        {
            UnitRegistry.Default.Add(unit);
            return unit;
        }

        // Length
        public static readonly Unit<Length> Meter = Register(new Unit<Length>("m", 1));
        public static readonly Unit<Length> Millimeter = Register(Milli * Meter);
        public static readonly Unit<Length> Centimeter = Register(Centi * Meter);
        public static readonly Unit<Length> Kilometer  = Register(Kilo * Meter);
        public static readonly Unit<Length> Inch = Register(new Unit<Length>("in", 2.54f, Centimeter));
        public static readonly Unit<Length> Foot = Register(new Unit<Length>("ft", 12, Inch));
        public static readonly Unit<Length> Yard = Register(new Unit<Length>("yd", 3, Foot));
        public static readonly Unit<Length> Mile = Register(new Unit<Length>("mi", 1760, Yard));

        // Mass
        public static readonly Unit<Mass> Gram = Register(new Unit<Mass>("g", 0.001));
        public static readonly Unit<Mass> Milligram = Register(Milli * Gram);
        public static readonly Unit<Mass> Kilogram = Register(Kilo * Gram);
        public static readonly Unit<Mass> Pound = Register(new Unit<Mass>("lb", 453.59237, Gram));

        // Time
        public static readonly Unit<Time> Second = Register(new Unit<Time>("s", 1));
        public static readonly Unit<Time> Minute = Register(new Unit<Time>("m", 60, Second));
        public static readonly Unit<Time> Hour = Register(new Unit<Time>("h", 60, Minute));

        // Temperature
        // U+00B0 DEGREE SIGN
        public static readonly Unit<Temperature> Kelvin = Register(new Unit<Temperature>("K", 1, 0));
        public static readonly Unit<Temperature> Celsius = Register(new Unit<Temperature>("\u00B0C", 1, 273.15));
        public static readonly Unit<Temperature> Fahrenheit = Register(new Unit<Temperature>("\u00B0F", 5 / 9.0, 273.15 - (32 * 5 / 9.0)));

        // Acceleration
        public static readonly Unit<Acceleration> Gravity = Register(new Unit<Acceleration>("g", 9.80665, Meter / Second / Second));

        // Energy
        public static readonly Unit<Energy> Joule = Register(new Unit<Energy>("J", 1));
        public static readonly Unit<Energy> Calorie = Register(new Unit<Energy>("cal", 4.184));
        public static readonly Unit<Energy> Btu = Register(new Unit<Energy>("btu", 1, Calorie * Pound / Gram * Fahrenheit / Celsius));

        // Force
        public static readonly Unit<Force> Newton = Register(new Unit<Force>("N", 1, Kilogram * Meter / Second / Second));
        public static readonly Unit<Force> Kilonewton = Register(Kilo * Newton);
        public static readonly Unit<Force> KilogramForce = Register(new Unit<Force>("kgf", 1, Kilogram * Gravity));
        public static readonly Unit<Force> PoundForce = Register(new Unit<Force>("lbf", 1, Pound * Gravity));

        // Power
        public static readonly Unit<Power> Watt = Register(new Unit<Power>("W", 1, Joule / Second));
        public static readonly Unit<Power> Kilowatt = Register(Kilo * Watt);
        public static readonly Unit<Power> MechanicalHorsepower = Register(new Unit<Power>("hp", 550, PoundForce * Foot / Second));

        // Pressure
        public static readonly Unit<Pressure> Pascal = Register(new Unit<Pressure>("Pa", 1, Newton / (Meter * Meter)));
        public static readonly Unit<Pressure> Bar = Register(new Unit<Pressure>("bar", 100, Kilo * Pascal));
        public static readonly Unit<Pressure> Psi = Register(new Unit<Pressure>("psi", 1, PoundForce / (Inch * Inch)));

        // Velocity
        public static readonly Unit<Velocity> MetersPerSecond = Register((Meter / Second).Assert<Velocity>());
        public static readonly Unit<Velocity> KilometersPerHour = Register((Kilometer / Hour).Assert<Velocity>());
        public static readonly Unit<Velocity> MilesPerHour = Register(new Unit<Velocity>("mph", 1, Mile / Hour));

        // Volume
        public static readonly Unit<Volume> Liter = Register(new Unit<Volume>("L", 1e-3, Meter * Meter * Meter));
        public static readonly Unit<Volume> USGallon = Register(new Unit<Volume>("gal", 231, Inch * Inch * Inch));
    }
}


