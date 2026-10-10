
using System;
using System.Collections.Generic;

namespace QuantitiesNet
{
    public static class Quantities
    {
        public static string GetName(Dimension d)
        {
            if (TypeForDimension.TryGetValue(d, out var dimensionType))
                return dimensionType.Name;
            return string.Empty;
        }

        public static readonly Dictionary<Dimension, Type> TypeForDimension = new Dictionary<Dimension, Type>()
        {
            { Dimension.ForType<Length>(), typeof(Length) },
            { Dimension.ForType<Mass>(), typeof(Mass) },
            { Dimension.ForType<Time>(), typeof(Time) },
            { Dimension.ForType<Temperature>(), typeof(Temperature) },
            { Dimension.ForType<AmountOfSubstance>(), typeof(AmountOfSubstance) },
            { Dimension.ForType<Area>(), typeof(Area) },
            { Dimension.ForType<Volume>(), typeof(Volume) },
            { Dimension.ForType<Density>(), typeof(Density) },
            { Dimension.ForType<MassFlow>(), typeof(MassFlow) },
            { Dimension.ForType<Velocity>(), typeof(Velocity) },
            { Dimension.ForType<Acceleration>(), typeof(Acceleration) },
            { Dimension.ForType<Force>(), typeof(Force) },
            { Dimension.ForType<Pressure>(), typeof(Pressure) },
            { Dimension.ForType<Energy>(), typeof(Energy) },
            { Dimension.ForType<Power>(), typeof(Power) },
            { Dimension.ForType<SpecificEnergy>(), typeof(SpecificEnergy) },
            { Dimension.ForType<SpecificHeatCapacity>(), typeof(SpecificHeatCapacity) },
            { Dimension.ForType<MolarMass>(), typeof(MolarMass) },
            { Dimension.ForType<MolarEnergy>(), typeof(MolarEnergy) },
            { Dimension.ForType<MolarHeatCapacity>(), typeof(MolarHeatCapacity) },

        };

        public sealed class Length : Quantity<Dimensions.Length>
        {
            public Length() : base() { }
            public Length(double scalar) : base(scalar) { }
            public Length(double scalar, Unit<Dimensions.Length> unit) : base(scalar, unit) { }

            public static SpecificEnergy operator * (Length lhs, Acceleration rhs) => new SpecificEnergy(lhs.Scalar * rhs.Scalar);
            public static Volume operator * (Length lhs, Area rhs) => new Volume(lhs.Scalar * rhs.Scalar);
            public static Energy operator * (Length lhs, Force rhs) => new Energy(lhs.Scalar * rhs.Scalar);
            public static Area operator * (Length lhs, Length rhs) => new Area(lhs.Scalar * rhs.Scalar);
            public static Velocity operator / (Length lhs, Time rhs) => new Velocity(lhs.Scalar / rhs.Scalar);
            public static Time operator / (Length lhs, Velocity rhs) => new Time(lhs.Scalar / rhs.Scalar);
        }

        public sealed class Mass : Quantity<Dimensions.Mass>
        {
            public Mass() : base() { }
            public Mass(double scalar) : base(scalar) { }
            public Mass(double scalar, Unit<Dimensions.Mass> unit) : base(scalar, unit) { }

            public static Force operator * (Mass lhs, Acceleration rhs) => new Force(lhs.Scalar * rhs.Scalar);
            public static MolarMass operator / (Mass lhs, AmountOfSubstance rhs) => new MolarMass(lhs.Scalar / rhs.Scalar);
            public static Volume operator / (Mass lhs, Density rhs) => new Volume(lhs.Scalar / rhs.Scalar);
            public static Time operator / (Mass lhs, MassFlow rhs) => new Time(lhs.Scalar / rhs.Scalar);
            public static AmountOfSubstance operator / (Mass lhs, MolarMass rhs) => new AmountOfSubstance(lhs.Scalar / rhs.Scalar);
            public static Energy operator * (Mass lhs, SpecificEnergy rhs) => new Energy(lhs.Scalar * rhs.Scalar);
            public static MassFlow operator / (Mass lhs, Time rhs) => new MassFlow(lhs.Scalar / rhs.Scalar);
            public static Density operator / (Mass lhs, Volume rhs) => new Density(lhs.Scalar / rhs.Scalar);
        }

        public sealed class Time : Quantity<Dimensions.Time>
        {
            public Time() : base() { }
            public Time(double scalar) : base(scalar) { }
            public Time(double scalar, Unit<Dimensions.Time> unit) : base(scalar, unit) { }

            public static Velocity operator * (Time lhs, Acceleration rhs) => new Velocity(lhs.Scalar * rhs.Scalar);
            public static Mass operator * (Time lhs, MassFlow rhs) => new Mass(lhs.Scalar * rhs.Scalar);
            public static Energy operator * (Time lhs, Power rhs) => new Energy(lhs.Scalar * rhs.Scalar);
            public static Length operator * (Time lhs, Velocity rhs) => new Length(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Temperature : Quantity<Dimensions.Temperature>
        {
            public Temperature() : base() { }
            public Temperature(double scalar) : base(scalar) { }
            public Temperature(double scalar, Unit<Dimensions.Temperature> unit) : base(scalar, unit) { }

            public static MolarEnergy operator * (Temperature lhs, MolarHeatCapacity rhs) => new MolarEnergy(lhs.Scalar * rhs.Scalar);
            public static SpecificEnergy operator * (Temperature lhs, SpecificHeatCapacity rhs) => new SpecificEnergy(lhs.Scalar * rhs.Scalar);
        }

        public sealed class AmountOfSubstance : Quantity<Dimensions.AmountOfSubstance>
        {
            public AmountOfSubstance() : base() { }
            public AmountOfSubstance(double scalar) : base(scalar) { }
            public AmountOfSubstance(double scalar, Unit<Dimensions.AmountOfSubstance> unit) : base(scalar, unit) { }

            public static Energy operator * (AmountOfSubstance lhs, MolarEnergy rhs) => new Energy(lhs.Scalar * rhs.Scalar);
            public static Mass operator * (AmountOfSubstance lhs, MolarMass rhs) => new Mass(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Area : Quantity<Dimensions.Area>
        {
            public Area() : base() { }
            public Area(double scalar) : base(scalar) { }
            public Area(double scalar, Unit<Dimensions.Area> unit) : base(scalar, unit) { }

            public static Volume operator * (Area lhs, Length rhs) => new Volume(lhs.Scalar * rhs.Scalar);
            public static Force operator * (Area lhs, Pressure rhs) => new Force(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Volume : Quantity<Dimensions.Volume>
        {
            public Volume() : base() { }
            public Volume(double scalar) : base(scalar) { }
            public Volume(double scalar, Unit<Dimensions.Volume> unit) : base(scalar, unit) { }

            public static Length operator / (Volume lhs, Area rhs) => new Length(lhs.Scalar / rhs.Scalar);
            public static Mass operator * (Volume lhs, Density rhs) => new Mass(lhs.Scalar * rhs.Scalar);
            public static Area operator / (Volume lhs, Length rhs) => new Area(lhs.Scalar / rhs.Scalar);
            public static Energy operator * (Volume lhs, Pressure rhs) => new Energy(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Density : Quantity<Dimensions.Density>
        {
            public Density() : base() { }
            public Density(double scalar) : base(scalar) { }
            public Density(double scalar, Unit<Dimensions.Density> unit) : base(scalar, unit) { }

            public static Pressure operator * (Density lhs, SpecificEnergy rhs) => new Pressure(lhs.Scalar * rhs.Scalar);
            public static Mass operator * (Density lhs, Volume rhs) => new Mass(lhs.Scalar * rhs.Scalar);
        }

        public sealed class MassFlow : Quantity<Dimensions.MassFlow>
        {
            public MassFlow() : base() { }
            public MassFlow(double scalar) : base(scalar) { }
            public MassFlow(double scalar, Unit<Dimensions.MassFlow> unit) : base(scalar, unit) { }

            public static Power operator * (MassFlow lhs, SpecificEnergy rhs) => new Power(lhs.Scalar * rhs.Scalar);
            public static Mass operator * (MassFlow lhs, Time rhs) => new Mass(lhs.Scalar * rhs.Scalar);
            public static Force operator * (MassFlow lhs, Velocity rhs) => new Force(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Velocity : Quantity<Dimensions.Velocity>
        {
            public Velocity() : base() { }
            public Velocity(double scalar) : base(scalar) { }
            public Velocity(double scalar, Unit<Dimensions.Velocity> unit) : base(scalar, unit) { }

            public static Time operator / (Velocity lhs, Acceleration rhs) => new Time(lhs.Scalar / rhs.Scalar);
            public static Power operator * (Velocity lhs, Force rhs) => new Power(lhs.Scalar * rhs.Scalar);
            public static Force operator * (Velocity lhs, MassFlow rhs) => new Force(lhs.Scalar * rhs.Scalar);
            public static Length operator * (Velocity lhs, Time rhs) => new Length(lhs.Scalar * rhs.Scalar);
            public static SpecificEnergy operator * (Velocity lhs, Velocity rhs) => new SpecificEnergy(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Acceleration : Quantity<Dimensions.Acceleration>
        {
            public Acceleration() : base() { }
            public Acceleration(double scalar) : base(scalar) { }
            public Acceleration(double scalar, Unit<Dimensions.Acceleration> unit) : base(scalar, unit) { }

            public static SpecificEnergy operator * (Acceleration lhs, Length rhs) => new SpecificEnergy(lhs.Scalar * rhs.Scalar);
            public static Force operator * (Acceleration lhs, Mass rhs) => new Force(lhs.Scalar * rhs.Scalar);
            public static Velocity operator * (Acceleration lhs, Time rhs) => new Velocity(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Force : Quantity<Dimensions.Force>
        {
            public Force() : base() { }
            public Force(double scalar) : base(scalar) { }
            public Force(double scalar, Unit<Dimensions.Force> unit) : base(scalar, unit) { }

            public static Mass operator / (Force lhs, Acceleration rhs) => new Mass(lhs.Scalar / rhs.Scalar);
            public static Pressure operator / (Force lhs, Area rhs) => new Pressure(lhs.Scalar / rhs.Scalar);
            public static Energy operator * (Force lhs, Length rhs) => new Energy(lhs.Scalar * rhs.Scalar);
            public static Acceleration operator / (Force lhs, Mass rhs) => new Acceleration(lhs.Scalar / rhs.Scalar);
            public static Velocity operator / (Force lhs, MassFlow rhs) => new Velocity(lhs.Scalar / rhs.Scalar);
            public static Area operator / (Force lhs, Pressure rhs) => new Area(lhs.Scalar / rhs.Scalar);
            public static Power operator * (Force lhs, Velocity rhs) => new Power(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Pressure : Quantity<Dimensions.Pressure>
        {
            public Pressure() : base() { }
            public Pressure(double scalar) : base(scalar) { }
            public Pressure(double scalar, Unit<Dimensions.Pressure> unit) : base(scalar, unit) { }

            public static Force operator * (Pressure lhs, Area rhs) => new Force(lhs.Scalar * rhs.Scalar);
            public static SpecificEnergy operator / (Pressure lhs, Density rhs) => new SpecificEnergy(lhs.Scalar / rhs.Scalar);
            public static Density operator / (Pressure lhs, SpecificEnergy rhs) => new Density(lhs.Scalar / rhs.Scalar);
            public static Energy operator * (Pressure lhs, Volume rhs) => new Energy(lhs.Scalar * rhs.Scalar);
        }

        public sealed class Energy : Quantity<Dimensions.Energy>
        {
            public Energy() : base() { }
            public Energy(double scalar) : base(scalar) { }
            public Energy(double scalar, Unit<Dimensions.Energy> unit) : base(scalar, unit) { }

            public static MolarEnergy operator / (Energy lhs, AmountOfSubstance rhs) => new MolarEnergy(lhs.Scalar / rhs.Scalar);
            public static Length operator / (Energy lhs, Force rhs) => new Length(lhs.Scalar / rhs.Scalar);
            public static Force operator / (Energy lhs, Length rhs) => new Force(lhs.Scalar / rhs.Scalar);
            public static SpecificEnergy operator / (Energy lhs, Mass rhs) => new SpecificEnergy(lhs.Scalar / rhs.Scalar);
            public static AmountOfSubstance operator / (Energy lhs, MolarEnergy rhs) => new AmountOfSubstance(lhs.Scalar / rhs.Scalar);
            public static Time operator / (Energy lhs, Power rhs) => new Time(lhs.Scalar / rhs.Scalar);
            public static Volume operator / (Energy lhs, Pressure rhs) => new Volume(lhs.Scalar / rhs.Scalar);
            public static Mass operator / (Energy lhs, SpecificEnergy rhs) => new Mass(lhs.Scalar / rhs.Scalar);
            public static Power operator / (Energy lhs, Time rhs) => new Power(lhs.Scalar / rhs.Scalar);
            public static Pressure operator / (Energy lhs, Volume rhs) => new Pressure(lhs.Scalar / rhs.Scalar);
        }

        public sealed class Power : Quantity<Dimensions.Power>
        {
            public Power() : base() { }
            public Power(double scalar) : base(scalar) { }
            public Power(double scalar, Unit<Dimensions.Power> unit) : base(scalar, unit) { }

            public static Velocity operator / (Power lhs, Force rhs) => new Velocity(lhs.Scalar / rhs.Scalar);
            public static SpecificEnergy operator / (Power lhs, MassFlow rhs) => new SpecificEnergy(lhs.Scalar / rhs.Scalar);
            public static MassFlow operator / (Power lhs, SpecificEnergy rhs) => new MassFlow(lhs.Scalar / rhs.Scalar);
            public static Energy operator * (Power lhs, Time rhs) => new Energy(lhs.Scalar * rhs.Scalar);
            public static Force operator / (Power lhs, Velocity rhs) => new Force(lhs.Scalar / rhs.Scalar);
        }

        public sealed class SpecificEnergy : Quantity<Dimensions.SpecificEnergy>
        {
            public SpecificEnergy() : base() { }
            public SpecificEnergy(double scalar) : base(scalar) { }
            public SpecificEnergy(double scalar, Unit<Dimensions.SpecificEnergy> unit) : base(scalar, unit) { }

            public static Length operator / (SpecificEnergy lhs, Acceleration rhs) => new Length(lhs.Scalar / rhs.Scalar);
            public static Pressure operator * (SpecificEnergy lhs, Density rhs) => new Pressure(lhs.Scalar * rhs.Scalar);
            public static Acceleration operator / (SpecificEnergy lhs, Length rhs) => new Acceleration(lhs.Scalar / rhs.Scalar);
            public static Energy operator * (SpecificEnergy lhs, Mass rhs) => new Energy(lhs.Scalar * rhs.Scalar);
            public static Power operator * (SpecificEnergy lhs, MassFlow rhs) => new Power(lhs.Scalar * rhs.Scalar);
            public static MolarEnergy operator * (SpecificEnergy lhs, MolarMass rhs) => new MolarEnergy(lhs.Scalar * rhs.Scalar);
            public static Temperature operator / (SpecificEnergy lhs, SpecificHeatCapacity rhs) => new Temperature(lhs.Scalar / rhs.Scalar);
            public static SpecificHeatCapacity operator / (SpecificEnergy lhs, Temperature rhs) => new SpecificHeatCapacity(lhs.Scalar / rhs.Scalar);
            public static Velocity operator / (SpecificEnergy lhs, Velocity rhs) => new Velocity(lhs.Scalar / rhs.Scalar);
        }

        public sealed class SpecificHeatCapacity : Quantity<Dimensions.SpecificHeatCapacity>
        {
            public SpecificHeatCapacity() : base() { }
            public SpecificHeatCapacity(double scalar) : base(scalar) { }
            public SpecificHeatCapacity(double scalar, Unit<Dimensions.SpecificHeatCapacity> unit) : base(scalar, unit) { }

            public static MolarHeatCapacity operator * (SpecificHeatCapacity lhs, MolarMass rhs) => new MolarHeatCapacity(lhs.Scalar * rhs.Scalar);
            public static SpecificEnergy operator * (SpecificHeatCapacity lhs, Temperature rhs) => new SpecificEnergy(lhs.Scalar * rhs.Scalar);
        }

        public sealed class MolarMass : Quantity<Dimensions.MolarMass>
        {
            public MolarMass() : base() { }
            public MolarMass(double scalar) : base(scalar) { }
            public MolarMass(double scalar, Unit<Dimensions.MolarMass> unit) : base(scalar, unit) { }

            public static Mass operator * (MolarMass lhs, AmountOfSubstance rhs) => new Mass(lhs.Scalar * rhs.Scalar);
            public static MolarEnergy operator * (MolarMass lhs, SpecificEnergy rhs) => new MolarEnergy(lhs.Scalar * rhs.Scalar);
            public static MolarHeatCapacity operator * (MolarMass lhs, SpecificHeatCapacity rhs) => new MolarHeatCapacity(lhs.Scalar * rhs.Scalar);
        }

        public sealed class MolarEnergy : Quantity<Dimensions.MolarEnergy>
        {
            public MolarEnergy() : base() { }
            public MolarEnergy(double scalar) : base(scalar) { }
            public MolarEnergy(double scalar, Unit<Dimensions.MolarEnergy> unit) : base(scalar, unit) { }

            public static Energy operator * (MolarEnergy lhs, AmountOfSubstance rhs) => new Energy(lhs.Scalar * rhs.Scalar);
            public static Temperature operator / (MolarEnergy lhs, MolarHeatCapacity rhs) => new Temperature(lhs.Scalar / rhs.Scalar);
            public static SpecificEnergy operator / (MolarEnergy lhs, MolarMass rhs) => new SpecificEnergy(lhs.Scalar / rhs.Scalar);
            public static MolarMass operator / (MolarEnergy lhs, SpecificEnergy rhs) => new MolarMass(lhs.Scalar / rhs.Scalar);
            public static MolarHeatCapacity operator / (MolarEnergy lhs, Temperature rhs) => new MolarHeatCapacity(lhs.Scalar / rhs.Scalar);
        }

        public sealed class MolarHeatCapacity : Quantity<Dimensions.MolarHeatCapacity>
        {
            public MolarHeatCapacity() : base() { }
            public MolarHeatCapacity(double scalar) : base(scalar) { }
            public MolarHeatCapacity(double scalar, Unit<Dimensions.MolarHeatCapacity> unit) : base(scalar, unit) { }

            public static SpecificHeatCapacity operator / (MolarHeatCapacity lhs, MolarMass rhs) => new SpecificHeatCapacity(lhs.Scalar / rhs.Scalar);
            public static MolarMass operator / (MolarHeatCapacity lhs, SpecificHeatCapacity rhs) => new MolarMass(lhs.Scalar / rhs.Scalar);
            public static MolarEnergy operator * (MolarHeatCapacity lhs, Temperature rhs) => new MolarEnergy(lhs.Scalar * rhs.Scalar);
        }
    }
}

