using System;
using System.Numerics;

namespace Osmium.Engine.Values.Numerics
{
    public sealed class RationalValue : NumberValue
    {
        public BigInteger Numerator { get; }
        public BigInteger Denominator { get; }

        public RationalValue(OsmiumEngine engine, BigInteger numerator, BigInteger denominator) : base(engine)
        {
            if (denominator.Sign < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            var gcd = GCD(numerator, denominator);
            if (gcd != 1)
            {
                numerator /= gcd;
                denominator /= gcd;
            }
            Numerator = numerator;
            Denominator = denominator;
        }

        private RationalValue(OsmiumEngine engine, BigInteger numerator, BigInteger denominator, bool unused) : base(engine)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        private static BigInteger GCD(BigInteger a, BigInteger b)
        {
            if (a < 0) a = -a;
            if (b < 0) b = -b;
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a == 0 ? b : a;
        }

        public override Value Head => Engine.System.Rational.ToValue();
        internal override NumberType Type => NumberType.Rational;
        internal override NumberValue ConvertTo(NumberType type)
        {
            switch (type)
            {
                case NumberType.Integer:
                    return new IntegerValue(Engine, Numerator / Denominator);
                case NumberType.Rational:
                    return this;
                case NumberType.Real:
                    return new RealValue(Engine, (double) Numerator / (double) Denominator);
                case NumberType.Complex:
                    return new ComplexValue(Engine, this);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        internal override NumberValue Neg()
        {
            return new RationalValue(Engine, -Numerator, Denominator, true);
        }

        internal override NumberValue Inv()
        {
            return new RationalValue(Engine, Denominator, Numerator).Normalize();
        }

        internal override NumberValue Normalize()
        {
            if (Denominator.IsOne)
                return new IntegerValue(Engine, Numerator);

            return this;
        }

        internal override bool NormalEquals(NumberValue other)
        {
            return other is RationalValue rv && rv.Numerator == Numerator && rv.Denominator == Denominator;
        }

        internal override int NormalHashCode()
        {
            throw new NotImplementedException();
        }

        public static NumberValue Add(RationalValue a, RationalValue b) => new RationalValue(a.Engine, a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
        public static NumberValue Mul(RationalValue a, RationalValue b) => new RationalValue(a.Engine, a.Numerator * b.Numerator, a.Denominator * b.Denominator);
        public static NumberValue Pow(RationalValue a, RationalValue b) => throw new NotImplementedException();
    }
}