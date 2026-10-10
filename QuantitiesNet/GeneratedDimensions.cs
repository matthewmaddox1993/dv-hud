
namespace QuantitiesNet
{
    public static class Dimensions
    {

        public sealed class Length : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 1);
            public Dimension Dimension => dimension;

            public static SpecificEnergy operator * (Length lhs, Acceleration rhs) => new SpecificEnergy();
            public static Volume operator * (Length lhs, Area rhs) => new Volume();
            public static Energy operator * (Length lhs, Force rhs) => new Energy();
            public static Area operator * (Length lhs, Length rhs) => new Area();
            public static Velocity operator / (Length lhs, Time rhs) => new Velocity();
            public static Time operator / (Length lhs, Velocity rhs) => new Time();
        }

        public sealed class Mass : IDimension
        {
            public static readonly Dimension dimension = new Dimension(mass: 1);
            public Dimension Dimension => dimension;

            public static Force operator * (Mass lhs, Acceleration rhs) => new Force();
            public static MolarMass operator / (Mass lhs, AmountOfSubstance rhs) => new MolarMass();
            public static Volume operator / (Mass lhs, Density rhs) => new Volume();
            public static Time operator / (Mass lhs, MassFlow rhs) => new Time();
            public static AmountOfSubstance operator / (Mass lhs, MolarMass rhs) => new AmountOfSubstance();
            public static Energy operator * (Mass lhs, SpecificEnergy rhs) => new Energy();
            public static MassFlow operator / (Mass lhs, Time rhs) => new MassFlow();
            public static Density operator / (Mass lhs, Volume rhs) => new Density();
        }

        public sealed class Time : IDimension
        {
            public static readonly Dimension dimension = new Dimension(time: 1);
            public Dimension Dimension => dimension;

            public static Velocity operator * (Time lhs, Acceleration rhs) => new Velocity();
            public static Mass operator * (Time lhs, MassFlow rhs) => new Mass();
            public static Energy operator * (Time lhs, Power rhs) => new Energy();
            public static Length operator * (Time lhs, Velocity rhs) => new Length();
        }

        public sealed class Temperature : IDimension
        {
            public static readonly Dimension dimension = new Dimension(temperature: 1);
            public Dimension Dimension => dimension;

            public static MolarEnergy operator * (Temperature lhs, MolarHeatCapacity rhs) => new MolarEnergy();
            public static SpecificEnergy operator * (Temperature lhs, SpecificHeatCapacity rhs) => new SpecificEnergy();
        }

        public sealed class AmountOfSubstance : IDimension
        {
            public static readonly Dimension dimension = new Dimension(amountOfSubstance: 1);
            public Dimension Dimension => dimension;

            public static Energy operator * (AmountOfSubstance lhs, MolarEnergy rhs) => new Energy();
            public static Mass operator * (AmountOfSubstance lhs, MolarMass rhs) => new Mass();
        }

        public sealed class Area : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 2);
            public Dimension Dimension => dimension;

            public static Volume operator * (Area lhs, Length rhs) => new Volume();
            public static Force operator * (Area lhs, Pressure rhs) => new Force();
        }

        public sealed class Volume : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 3);
            public Dimension Dimension => dimension;

            public static Length operator / (Volume lhs, Area rhs) => new Length();
            public static Mass operator * (Volume lhs, Density rhs) => new Mass();
            public static Area operator / (Volume lhs, Length rhs) => new Area();
            public static Energy operator * (Volume lhs, Pressure rhs) => new Energy();
        }

        public sealed class Density : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: -3, mass: 1);
            public Dimension Dimension => dimension;

            public static Pressure operator * (Density lhs, SpecificEnergy rhs) => new Pressure();
            public static Mass operator * (Density lhs, Volume rhs) => new Mass();
        }

        public sealed class MassFlow : IDimension
        {
            public static readonly Dimension dimension = new Dimension(mass: 1, time: -1);
            public Dimension Dimension => dimension;

            public static Power operator * (MassFlow lhs, SpecificEnergy rhs) => new Power();
            public static Mass operator * (MassFlow lhs, Time rhs) => new Mass();
            public static Force operator * (MassFlow lhs, Velocity rhs) => new Force();
        }

        public sealed class Velocity : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 1, time: -1);
            public Dimension Dimension => dimension;

            public static Time operator / (Velocity lhs, Acceleration rhs) => new Time();
            public static Power operator * (Velocity lhs, Force rhs) => new Power();
            public static Force operator * (Velocity lhs, MassFlow rhs) => new Force();
            public static Length operator * (Velocity lhs, Time rhs) => new Length();
            public static SpecificEnergy operator * (Velocity lhs, Velocity rhs) => new SpecificEnergy();
        }

        public sealed class Acceleration : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 1, time: -2);
            public Dimension Dimension => dimension;

            public static SpecificEnergy operator * (Acceleration lhs, Length rhs) => new SpecificEnergy();
            public static Force operator * (Acceleration lhs, Mass rhs) => new Force();
            public static Velocity operator * (Acceleration lhs, Time rhs) => new Velocity();
        }

        public sealed class Force : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 1, mass: 1, time: -2);
            public Dimension Dimension => dimension;

            public static Mass operator / (Force lhs, Acceleration rhs) => new Mass();
            public static Pressure operator / (Force lhs, Area rhs) => new Pressure();
            public static Energy operator * (Force lhs, Length rhs) => new Energy();
            public static Acceleration operator / (Force lhs, Mass rhs) => new Acceleration();
            public static Velocity operator / (Force lhs, MassFlow rhs) => new Velocity();
            public static Area operator / (Force lhs, Pressure rhs) => new Area();
            public static Power operator * (Force lhs, Velocity rhs) => new Power();
        }

        public sealed class Pressure : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: -1, mass: 1, time: -2);
            public Dimension Dimension => dimension;

            public static Force operator * (Pressure lhs, Area rhs) => new Force();
            public static SpecificEnergy operator / (Pressure lhs, Density rhs) => new SpecificEnergy();
            public static Density operator / (Pressure lhs, SpecificEnergy rhs) => new Density();
            public static Energy operator * (Pressure lhs, Volume rhs) => new Energy();
        }

        public sealed class Energy : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 2, mass: 1, time: -2);
            public Dimension Dimension => dimension;

            public static MolarEnergy operator / (Energy lhs, AmountOfSubstance rhs) => new MolarEnergy();
            public static Length operator / (Energy lhs, Force rhs) => new Length();
            public static Force operator / (Energy lhs, Length rhs) => new Force();
            public static SpecificEnergy operator / (Energy lhs, Mass rhs) => new SpecificEnergy();
            public static AmountOfSubstance operator / (Energy lhs, MolarEnergy rhs) => new AmountOfSubstance();
            public static Time operator / (Energy lhs, Power rhs) => new Time();
            public static Volume operator / (Energy lhs, Pressure rhs) => new Volume();
            public static Mass operator / (Energy lhs, SpecificEnergy rhs) => new Mass();
            public static Power operator / (Energy lhs, Time rhs) => new Power();
            public static Pressure operator / (Energy lhs, Volume rhs) => new Pressure();
        }

        public sealed class Power : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 2, mass: 1, time: -3);
            public Dimension Dimension => dimension;

            public static Velocity operator / (Power lhs, Force rhs) => new Velocity();
            public static SpecificEnergy operator / (Power lhs, MassFlow rhs) => new SpecificEnergy();
            public static MassFlow operator / (Power lhs, SpecificEnergy rhs) => new MassFlow();
            public static Energy operator * (Power lhs, Time rhs) => new Energy();
            public static Force operator / (Power lhs, Velocity rhs) => new Force();
        }

        public sealed class SpecificEnergy : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 2, time: -2);
            public Dimension Dimension => dimension;

            public static Length operator / (SpecificEnergy lhs, Acceleration rhs) => new Length();
            public static Pressure operator * (SpecificEnergy lhs, Density rhs) => new Pressure();
            public static Acceleration operator / (SpecificEnergy lhs, Length rhs) => new Acceleration();
            public static Energy operator * (SpecificEnergy lhs, Mass rhs) => new Energy();
            public static Power operator * (SpecificEnergy lhs, MassFlow rhs) => new Power();
            public static MolarEnergy operator * (SpecificEnergy lhs, MolarMass rhs) => new MolarEnergy();
            public static Temperature operator / (SpecificEnergy lhs, SpecificHeatCapacity rhs) => new Temperature();
            public static SpecificHeatCapacity operator / (SpecificEnergy lhs, Temperature rhs) => new SpecificHeatCapacity();
            public static Velocity operator / (SpecificEnergy lhs, Velocity rhs) => new Velocity();
        }

        public sealed class SpecificHeatCapacity : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 2, time: -2, temperature: -1);
            public Dimension Dimension => dimension;

            public static MolarHeatCapacity operator * (SpecificHeatCapacity lhs, MolarMass rhs) => new MolarHeatCapacity();
            public static SpecificEnergy operator * (SpecificHeatCapacity lhs, Temperature rhs) => new SpecificEnergy();
        }

        public sealed class MolarMass : IDimension
        {
            public static readonly Dimension dimension = new Dimension(mass: 1, amountOfSubstance: -1);
            public Dimension Dimension => dimension;

            public static Mass operator * (MolarMass lhs, AmountOfSubstance rhs) => new Mass();
            public static MolarEnergy operator * (MolarMass lhs, SpecificEnergy rhs) => new MolarEnergy();
            public static MolarHeatCapacity operator * (MolarMass lhs, SpecificHeatCapacity rhs) => new MolarHeatCapacity();
        }

        public sealed class MolarEnergy : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 2, mass: 1, time: -2, amountOfSubstance: -1);
            public Dimension Dimension => dimension;

            public static Energy operator * (MolarEnergy lhs, AmountOfSubstance rhs) => new Energy();
            public static Temperature operator / (MolarEnergy lhs, MolarHeatCapacity rhs) => new Temperature();
            public static SpecificEnergy operator / (MolarEnergy lhs, MolarMass rhs) => new SpecificEnergy();
            public static MolarMass operator / (MolarEnergy lhs, SpecificEnergy rhs) => new MolarMass();
            public static MolarHeatCapacity operator / (MolarEnergy lhs, Temperature rhs) => new MolarHeatCapacity();
        }

        public sealed class MolarHeatCapacity : IDimension
        {
            public static readonly Dimension dimension = new Dimension(length: 2, mass: 1, time: -2, amountOfSubstance: -1, temperature: -1);
            public Dimension Dimension => dimension;

            public static SpecificHeatCapacity operator / (MolarHeatCapacity lhs, MolarMass rhs) => new SpecificHeatCapacity();
            public static MolarMass operator / (MolarHeatCapacity lhs, SpecificHeatCapacity rhs) => new MolarMass();
            public static MolarEnergy operator * (MolarHeatCapacity lhs, Temperature rhs) => new MolarEnergy();
        }
    }
}


