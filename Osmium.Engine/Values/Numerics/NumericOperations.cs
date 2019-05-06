using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Osmium.Engine.Values.Numerics
{
    public static class NumericOperations
    {
        public delegate NumberValue NumericFunction(NumberValue a, NumberValue b);

        private static NumericFunction MakeOp<T>(Func<T, T, NumberValue> op) where T: NumberValue
        {
            return (a,  b) => op((T) a, (T) b);
        }

        private static readonly NumericFunction[] Adds = new[]
        {
            MakeOp<IntegerValue>(IntegerValue.Add),
            MakeOp<RationalValue>(RationalValue.Add),
            MakeOp<RealValue>(RealValue.Add),
            MakeOp<ComplexValue>(ComplexValue.Add)
        };

        private static readonly NumericFunction[] Muls = new[]
        {
            MakeOp<IntegerValue>(IntegerValue.Mul),
            MakeOp<RationalValue>(RationalValue.Mul),
            MakeOp<RealValue>(RealValue.Mul),
            MakeOp<ComplexValue>(ComplexValue.Mul)
        };



        private static NumericFunction MakeConvertorOp(NumericFunction[] funs)
        {
            NumberValue Helper(NumberValue a, NumberValue b)
            {
                if (a.Type == b.Type)
                    return funs[(int) a.Type](a, b);

                if (a.Type > b.Type)
                    return funs[(int) a.Type](a, b.ConvertTo(a.Type));

                return funs[(int) b.Type](a.ConvertTo(b.Type), b);
            }

            return Helper;
        }

        public static readonly NumericFunction Add = MakeConvertorOp(Adds);
        public static readonly NumericFunction Mul = MakeConvertorOp(Muls);

        public static (Value, bool)? Plus(OsmiumEngine engine, ExpressionValue value)
        {
            if (!(value.Head is Symbol sv) || sv != engine.System.Plus)
                return null;

            NumberValue nv = null;

            int numCount = 0;

            var nonNum = new List<Value>();

            foreach (var part in value)
            {
                if (!(part is NumberValue numPart))
                {
                    nonNum.Add(part);
                    continue;
                }

                numCount++;

                nv = nv == null ? numPart : Add(nv, numPart);
            }

            if (numCount < 2)
            {
                return (value, false);
            }

            if (nonNum.Count == 0)
            {
                return (nv, true);
            }

            nonNum.Add(nv);
            return (engine.Expr(value.Head, nonNum.ToArray()), true);
        }
    }
}
