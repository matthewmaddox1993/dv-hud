using System;
using System.Collections.Generic;

namespace QuantitiesNet
{
    public class Quantity
    {
        public double Scalar { get; }
        public virtual Dimension Dimension { get; }

        public Quantity(double scalar, Dimension dimension)
        {
            this.Scalar = scalar;
            this.Dimension = dimension;
        }

        public Quantity(double scalar, Unit unit)
        {
            this.Scalar = (scalar * unit.Scalar) + unit.Offset;
            this.Dimension = unit.Dimension;
        }

        public override bool Equals(object obj)
        {
            return obj is Quantity quantity &&
                   Scalar == quantity.Scalar &&
                   EqualityComparer<Dimension>.Default.Equals(Dimension, quantity.Dimension);
        }

        public override int GetHashCode()
        {
            int hashCode = -689933752;
            hashCode = hashCode * -1521134295 + Scalar.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Dimension>.Default.GetHashCode(Dimension);
            return hashCode;
        }

        public static bool operator ==(Quantity q1, Quantity q2)
        {
            if (q1 is null && q2 is null)
                return true;
            if (q1 is null || q2 is null)
                return false;
            return q1.Equals(q2);
        }

        public static bool operator !=(Quantity q1, Quantity q2)
        {
            return !(q1 == q2);
        }

        public double In(Unit unit)
        {
            if (Dimension != unit.Dimension)
                throw new ArgumentException($"dimensions do not match: {Dimension}, {unit.Dimension}");
            return (Scalar - unit.Offset) / unit.Scalar;
        }

       public static Quantity operator +(Quantity q1, Quantity q2)
        {
            if (q1.Dimension != q2.Dimension)
                throw new ArgumentException($"dimensions do not match: {q1.Dimension}, {q2.Dimension}");
            return new Quantity(q1.Scalar + q2.Scalar, q1.Dimension);
        }

        public static Quantity operator -(Quantity q1, Quantity q2)
        {
            if (q1.Dimension != q2.Dimension)
                throw new ArgumentException($"dimensions do not match: {q1.Dimension}, {q2.Dimension}");
            return new Quantity(q1.Scalar - q2.Scalar, q1.Dimension);
        }

        public static Quantity operator *(Quantity q1, Quantity q2)
        {
            return new Quantity(q1.Scalar * q2.Scalar, q1.Dimension * q2.Dimension);
        }

        public static Quantity operator /(Quantity q1, Quantity q2)
        {
            return new Quantity(q1.Scalar / q2.Scalar, q1.Dimension / q2.Dimension);
        }
    }

    public static class QuantityExtensions
    {
        public static Quantity Assert(this Quantity q, Dimension expected)
        {
            if (q.Dimension != expected)
                throw new ArgumentException($"dimension does not match expectation: {q.Dimension}");
            return q;
        }

        public static Quantity<D> Assert<D>(this Quantity q) where D : IDimension, new()
        {
            if (q.Dimension != Dimension.ForType<D>())
                throw new ArgumentException($"dimension does not match expectation: {q.Dimension}");
            return new Quantity<D>(q.Scalar);
        }
    }

    public class Quantity<D> : Quantity, IDimension
        where D : IDimension, new()
    {
        public Quantity() : base (default, Dimension.ForType<D>())
        {
        }

        public Quantity(double scalar) : base(scalar, Dimension.ForType<D>())
        {
        }

        // public Quantity(double scalar, Unit unit) : base(scalar, unit)
        // {
        //     if (unit.Dimension != Dimension.ForType<D>())
        //         throw new ArgumentException($"dimension does not match expectation: {unit.Dimension}");
        // }

        public Quantity(double scalar, Unit<D> unit) : base(scalar, unit)
        {
        }

        public override Dimension Dimension => Dimension.ForType<D>();

        public static Quantity<D> operator +(Quantity<D> q, Quantity<D> q2)
        {
            return new Quantity<D>(q.Scalar + q2.Scalar);
        }

        public static Quantity<D> operator -(Quantity<D> q, Quantity<D> q2)
        {
            return new Quantity<D>(q.Scalar - q2.Scalar);
        }

        public static bool operator < (Quantity<D> lhs, Quantity<D> rhs) => lhs.Scalar < rhs.Scalar;
        public static bool operator <= (Quantity<D> lhs, Quantity<D> rhs) => lhs.Scalar <= rhs.Scalar;
        public static bool operator > (Quantity<D> lhs, Quantity<D> rhs) => lhs.Scalar > rhs.Scalar;
        public static bool operator >= (Quantity<D> lhs, Quantity<D> rhs) => lhs.Scalar >= rhs.Scalar;
    }
}


