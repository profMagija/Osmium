using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Osmium.Engine.Values
{
    public sealed class StringValue : AtomicValue
    {
        public override Value Head => Engine.System.String.ToValue();
        public string Value { get; }

        public StringValue(OsmiumEngine engine, string value) : base(engine)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private bool Equals(StringValue other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is StringValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return  "\"" + Value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }

    public abstract class NumberValue : AtomicValue
    {
        protected NumberValue(OsmiumEngine engine) : base(engine)
        {
        }
    }

    public sealed class IntegerValue : NumberValue
    {
        public BigInteger Value { get; }

        public IntegerValue(OsmiumEngine engine, BigInteger value) : base(engine)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override Value Head => Engine.System.Integer.ToValue();
    }

    public sealed class RationalValue : NumberValue
    {
        public BigInteger Numerator { get; }
        public BigInteger Denominator { get; }

        public RationalValue(OsmiumEngine engine, BigInteger numerator, BigInteger denominator) : base(engine)
        {
            var gcd = GCD(numerator, denominator);
            if (gcd != 1)
            {
                numerator /= gcd;
                denominator /= gcd;
            }
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

        public override Value Head { get; }
    }
}
