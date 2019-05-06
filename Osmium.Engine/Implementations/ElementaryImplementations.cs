using System;
using System.Collections.Generic;
using System.Linq;
using Osmium.Engine.Values;

namespace Osmium.Engine.Implementations
{
    internal static class ElementaryImplementations
    {
        public static (Value, bool)? And(OsmiumEngine engine, ExpressionValue expr)
        {
            var pts = new List<Value>();

            bool noChange = true;

            foreach (var part in expr.Parts)
            {
                var epart = engine.Evaluate(part);
                if (epart is Symbol sv)
                {
                    if (sv == engine.System.False)
                    {
                        return (epart, false);
                    }

                    if (sv == engine.System.True)
                    {
                        continue;
                    }
                }

                noChange = noChange && epart.Equals(part);

                pts.Add(epart);
            }

            if (pts.Count == 0)
                return (engine.System.True, false);

            if (pts.Count == 1)
                return (pts[0], false);

            return (engine.Expr(engine.System.And,
                pts.ToArray()
            ), false);
        }

        public static (Value, bool)? Append(OsmiumEngine engine, ExpressionValue expr)
        {
            if (!expr.Head.Equals(engine.System.Append))
                return null;
            if (expr.Count != 2)
            {
                // TODO: message
                return null;
            }

            var list = expr[0];
            var toAppend = expr[1];

            if (list is ExpressionValue e)
            {
                var newParts = new Value[e.Count + 1];
                Array.Copy(e.Parts.ToArray(), newParts, e.Count);
                newParts[e.Count] = toAppend;

                return (engine.Expr(e.Head, newParts), true);
            }

            return null;
        }

        private static Value DoApply(OsmiumEngine engine, Value f, Value target, int lvlFrom, int lvlTo)
        {
            if (target is ExpressionValue expr)
                return engine.Expr(f, expr.Parts.ToArray());
            return target;
        }

        public static (Value, bool)? Apply(OsmiumEngine engine, ExpressionValue expr)
        {
            if (expr.Head is ExpressionValue hexpr)
            {
                if (hexpr.Head is Symbol sv && sv == engine.System.Apply && hexpr.Count == 1 && expr.Count == 1)
                {
                    return (DoApply(engine, hexpr[0], expr[0], 0, 0), true);
                }
            }
            else if (expr.Head is Symbol sv && sv == engine.System.Apply)
            {
                if (expr.Count == 1)
                    return null;

                if (expr.Count == 2)
                    return (DoApply(engine, expr[0], expr[1], 0, 0), true);

                // TODO levelspec
            }

            return null;
        }
    }
}