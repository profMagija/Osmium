using System;
using System.Numerics;

namespace Osmium.Engine.Values.Numerics
{
    public sealed class RealValue : NumberValue
    {
        public double Value { get; }

        public RealValue(OsmiumEngine engine, double value) : base(engine)
        {
            Value = value;
        }

        public override Value Head => Engine.System.Real.ToValue();
        internal override NumberType Type => NumberType.Real;
        internal override NumberValue ConvertTo(NumberType type)
        {
            switch (type)
            {
                case NumberType.Integer:
                    return new IntegerValue(Engine, (BigInteger) Value);
                case NumberType.Rational:
                    throw new NotImplementedException();
                case NumberType.Real:
                    return this;
                case NumberType.Complex:
                    return new ComplexValue(Engine, this);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        internal override NumberValue Neg()
        {
            return new RealValue(Engine, -Value);
        }

        internal override NumberValue Inv()
        {
            return new RealValue(Engine, 1 / Value);
        }

        internal override NumberValue Normalize()
        {
            return this;
        }

        internal override bool NormalEquals(NumberValue other)
        {
            return other is RealValue rv && rv.Value == Value;
        }

        internal override int NormalHashCode()
        {
            return Value.GetHashCode();
        }

        public static NumberValue Add(RealValue a, RealValue b) => new RealValue(a.Engine, a.Value + b.Value);
        public static NumberValue Mul(RealValue a, RealValue b) => new RealValue(a.Engine, a.Value * b.Value);
        public static NumberValue Pow(RealValue a, RealValue b) => new RealValue(a.Engine, Math.Pow(a.Value, b.Value));
    }
}
