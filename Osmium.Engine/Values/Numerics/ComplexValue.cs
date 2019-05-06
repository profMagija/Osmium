using System;

namespace Osmium.Engine.Values.Numerics
{
    public sealed class ComplexValue : NumberValue
    {
        public NumberValue Real { get; }
        public NumberValue Imag { get; }
        public ComplexValue(OsmiumEngine engine, NumberValue real = null, NumberValue complex = null) : base(engine)
        {
            Real = real ?? Engine.Zero;
            Imag = complex ?? Engine.Zero;
        }

        public override Value Head => Engine.System.Complex;
        internal override NumberType Type => NumberType.Complex;
        internal override NumberValue ConvertTo(NumberType type)
        {
            switch (type)
            {
                case NumberType.Integer:
                case NumberType.Rational:
                case NumberType.Real:
                    return Real.ConvertTo(type);
                case NumberType.Complex:
                    return this;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        internal override NumberValue Neg()
        {
            return new ComplexValue(Engine, Real.Neg(), Imag.Neg());
        }

        internal override NumberValue Inv()
        {
            var fac = Real * Real + Imag * Imag;
            return new ComplexValue(Engine, Real / fac, Imag.Neg() / fac);
        }

        internal override NumberValue Normalize()
        {
            var imagNormal = Imag.Normalize();
            if (imagNormal is IntegerValue i && i.Value.IsZero)
                return Real.Normalize();

            return new ComplexValue(Engine, Real.Normalize(), imagNormal);
        }

        internal override bool NormalEquals(NumberValue other)
        {
            return other is ComplexValue cv && cv.Real.Equals(Real) && cv.Imag.Equals(Imag);
        }

        internal override int NormalHashCode()
        {
            return unchecked((Real.GetHashCode() * 937) ^ Imag.GetHashCode());
        }

        public static NumberValue Add(ComplexValue a, ComplexValue b)
        {
            return new ComplexValue(a.Engine, a.Real + b.Real, a.Imag + b.Imag);
        }

        public static NumberValue Mul(ComplexValue a, ComplexValue b)
        {
            return new ComplexValue(a.Engine, a.Real * b.Real + (a.Imag * b.Imag).Neg(), a.Real * b.Imag + a.Imag * b.Real);
        }
    }
}