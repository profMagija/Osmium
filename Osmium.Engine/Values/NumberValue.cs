using Osmium.Engine.Values.Numerics;

namespace Osmium.Engine.Values
{
    internal enum NumberType
    {
        Integer = 0,
        Rational = 1,
        Real = 2,
        Complex = 3
    }

    public abstract class NumberValue : AtomicValue
    {
        internal abstract NumberType Type { get; }
        protected NumberValue(OsmiumEngine engine) : base(engine)
        {
        }

        internal abstract NumberValue ConvertTo(NumberType type);

        internal abstract NumberValue Neg();
        internal abstract NumberValue Inv();
        internal abstract NumberValue Normalize();
        internal abstract bool NormalEquals(NumberValue other);
        internal abstract int NormalHashCode();

        protected bool Equals(NumberValue other)
        {
            return Normalize().NormalEquals(other.Normalize());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is NumberValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Normalize().NormalHashCode();
        }

        public static NumberValue operator +(NumberValue a, NumberValue b) => NumericOperations.Add(a, b);
        public static NumberValue operator -(NumberValue a, NumberValue b) => NumericOperations.Add(a, b.Neg());
        public static NumberValue operator *(NumberValue a, NumberValue b) => NumericOperations.Mul(a, b);
        public static NumberValue operator /(NumberValue a, NumberValue b) => NumericOperations.Mul(a, b.Inv());
    }
}