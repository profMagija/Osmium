using System.Collections.Generic;
using Osmium.Engine.Patterns.PatternClasses;
using Osmium.Engine.Values;

namespace Osmium.Engine.Implementations
{
    internal class SetImplementations   
    {
        public static (Value, bool)? SetDelayed(OsmiumEngine engine, ExpressionValue expr)
        {
            if (expr.Count != 2)
            {
                // TODO: message
            }

            var lhs = expr[0];
            var rhs = expr[1];

            switch (lhs)
            {
                case ExpressionValue lhsExpr:
                    // TODO: make sure head is a symbol
                    DoSet(engine.PatternMatching.CompilePattern(lhsExpr), lhsExpr.SymbolHead, lhsExpr.SymbolHead.DownValues, rhs);
                    break;
                case Symbol lhsSymbol:
                    DoSet(engine.PatternMatching.CompilePattern(lhsSymbol), lhsSymbol, lhsSymbol.OwnValues, rhs);
                    break;
                default:
                    // TODO: message
                    break;
            }

            return (engine.System.Null.ToValue(), false);
        }

        private static void DoSet(Pattern pattern, Symbol symbol, List<(Pattern, Value)> targetValues, Value value)
        {
            if (symbol.Attributes.HasFlag(SymbolAttributes.Protected))
            {
                // TODO: message
                return;
            }

            targetValues.Insert(0, (pattern, value));
        }

        public static (Value, bool)? Set(OsmiumEngine engine, ExpressionValue expr)
        {
            if (expr.Count != 2)
            {
                // TODO: message
            }

            var lhs = expr[0];
            var rhs = expr[1];

            switch (lhs)
            {
                case ExpressionValue lhsExpr:
                    // TODO: make sure head is a symbol
                    // TODO: allow {x, y} = {1, 2}
                    DoSet(engine.PatternMatching.CompilePattern(lhsExpr), lhsExpr.SymbolHead, lhsExpr.SymbolHead.DownValues, rhs);
                    break;
                case Symbol lhsSymbol:
                    DoSet(engine.PatternMatching.CompilePattern(lhsSymbol), lhsSymbol, lhsSymbol.OwnValues, rhs);
                    break;
                default:
                    // TODO: message
                    break;
            }

            return (rhs, false);
        }
    }
}