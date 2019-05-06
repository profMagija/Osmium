using System;
using System.Numerics;

namespace Osmium.Engine.Values.Numerics
{
    public sealed class IntegerValue : NumberValue
    {
        internal override NumberType Type => NumberType.Integer;

        public BigInteger Value { get; }

        public IntegerValue(OsmiumEngine engine, BigInteger value) : base(engine)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override Value Head => Engine.System.Integer;

        internal override NumberValue ConvertTo(NumberType type)
        {
            switch (type)
            {
                case NumberType.Integer:
                    return this;
                case NumberType.Rational:
                    return new RationalValue(Engine, Value, 1);
                case NumberType.Real:
                    return new RealValue(Engine, (double) Value);
                case NumberType.Complex:
                    return new ComplexValue(Engine, this);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        internal override NumberValue Neg()
        {
            return new IntegerValue(Engine, -Value);
        }

        internal override NumberValue Inv()
        {
            if (Value.IsOne)
                return this;
            if (Value == -1)
                return this;
            return new RationalValue(Engine, 1, Value).Normalize();
        }

        internal override NumberValue Normalize()
        {
            return this;
        }

        internal override bool NormalEquals(NumberValue other)
        {
            return other is IntegerValue iv && iv.Value == Value;
        }

        internal override int NormalHashCode()
        {
            return Value.GetHashCode();
        }

        public static IntegerValue Add(IntegerValue a, IntegerValue b) => new IntegerValue(a.Engine, a.Value + b.Value);
        public static IntegerValue Mul(IntegerValue a, IntegerValue b) => new IntegerValue(a.Engine, a.Value * b.Value);

        private static BigInteger Pow(BigInteger a, BigInteger exp)
        {
            var r = BigInteger.One;
            while (!exp.IsZero)
            {
                r *= r;
                if (!exp.IsEven) r *= a;
                exp >>= 1;
            }

            return r;
        }

        public static NumberValue Pow(IntegerValue a, IntegerValue b)
        {
            if (b.Value.Sign > 0)
                return new IntegerValue(a.Engine, Pow(a.Value, b.Value));
            if (b.Value.Sign < 0)
                return new RationalValue(a.Engine, 1, Pow(a.Value, -b.Value));
            if (a.Value.IsZero)
            {
                // TODO: indeterminate
            }

            return new IntegerValue(a.Engine, BigInteger.One);
        }
    }
}