using System;
using System.Collections.Generic;
using System.Text;
using Osmium.Engine.Values;
using Osmium.Engine.Values.Numerics;

namespace Osmium.Engine.Implementations
{
    public static class NumericImplementations
    {
        public static (Value, bool)? Plus(OsmiumEngine engine, ExpressionValue value)
        {
            if (!(value.Head is Symbol sv) || sv != engine.System.Plus)
                return null;

            return NumericOp(engine, value, engine.Zero, NumericOperations.Add);
        }

        public static (Value, bool)? Times(OsmiumEngine engine, ExpressionValue value)
        {
            if (!(value.Head is Symbol sv) || sv != engine.System.Times)
                return null;

            if (value.Count == 2 && value[0].Equals(engine.MinusOne) && value[1] is NumberValue number)
            {
                return (number.Neg(), true); // short-circuit form for Times[-1, x]
            }

            return NumericOp(engine, value, engine.Zero, NumericOperations.Mul);
        }

        private static (Value, bool)? NumericOp(OsmiumEngine engine, ExpressionValue value, NumberValue identity, NumericOperations.NumericFunction op)
        {
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

                nv = nv == null ? numPart : op(nv, numPart);
            }

            if (nonNum.Count == 0) // there were no non-numbers ( 1 + 2 ) -> 3
            {
                return (nv, true);
            }

            if (nv == null) //  no number was found (x + y)
                return (value, false);

            if (nv.Equals(identity)) // total of numbers was the identity element ( x + y + 1 - 1 ) -> ( x + y )
            {
                if (nonNum.Count == 1) // exactly one non-number ( x + 1 - 1 ) -> x
                {
                    return (nonNum[0], true);
                }

                // multiple non-numbers, keep the Plus head
                return (engine.Expr(value.Head, nonNum.ToArray()), true);
            }

            if (numCount == 1) // there was exactly one number, nothing changed  (x + 1) -> (x + 1)
                return (value, false);

            // there were multiple numbers, add the result and keep the rest (x + y + 1 + 2) -> (x + y + 3)
            nonNum.Add(nv);
            return (engine.Expr(value.Head, nonNum.ToArray()), true);
        }
    }
}
