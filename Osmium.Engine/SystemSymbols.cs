using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Osmium.Engine.Patterns;
using Osmium.Engine.Patterns.PatternClasses;
using Osmium.Engine.Values;

namespace Osmium.Engine
{
    partial class SystemSymbols
    {
        private void Init()
        {
            Set.DownCode = SetImplementations.Set;
            Set.Attributes = SymbolAttributes.HoldFirst | SymbolAttributes.Protected | SymbolAttributes.SequenceHold;
            SetDelayed.DownCode = SetImplementations.SetDelayed;
            SetDelayed.Attributes = SymbolAttributes.HoldAll | SymbolAttributes.Protected | SymbolAttributes.SequenceHold;
        }
    }

    internal class SetImplementations   
    {
        public static Value SetDelayed(OsmiumEngine engine, Value value)
        {
            var expr = (ExpressionValue) value;
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
                case SymbolValue lhsSymbol:
                    DoSet(engine.PatternMatching.CompilePattern(lhsSymbol), lhsSymbol.Symbol, lhsSymbol.Symbol.OwnValues, rhs);
                    break;
                default:
                    // TODO: message
                    break;
            }

            return engine.System.Null.ToValue();
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

        public static Value Set(OsmiumEngine engine, Value value)
        {
            var expr = (ExpressionValue)value;
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
                case SymbolValue lhsSymbol:
                    DoSet(engine.PatternMatching.CompilePattern(lhsSymbol), lhsSymbol.Symbol, lhsSymbol.Symbol.OwnValues, rhs);
                    break;
                default:
                    // TODO: message
                    break;
            }

            return rhs;
        }
    }
}
