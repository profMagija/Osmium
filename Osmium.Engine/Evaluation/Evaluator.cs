using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using Osmium.Engine.Patterns;
using Osmium.Engine.Values;

namespace Osmium.Engine.Evaluation
{
    internal class Evaluator
    {
        private OsmiumEngine _engine;

        public Evaluator(OsmiumEngine engine)
        {
            _engine = engine;
        }

        private (Value, bool) EvaluateExpression(ExpressionValue expr)
        {
            var head = Evaluate(expr.Head);
            // sym is just the head, if it's a symbol
            var sym = (head as SymbolValue)?.Symbol;
            var attrs = sym?.Attributes ?? 0;

            var evalFirst = true;
            var evalRest = true;

            if (attrs.HasFlag(SymbolAttributes.HoldAll) || attrs.HasFlag(SymbolAttributes.HoldAllComplete))
            {
                evalFirst = false;
                evalRest = false;
            }

            if (attrs.HasFlag(SymbolAttributes.HoldFirst))
                evalFirst = false;

            if (attrs.HasFlag(SymbolAttributes.HoldRest))
                evalRest = false;

            var parts = new Value[expr.Count];

            for (var i = 0; i < expr.Count; i++)
            {
                if ((i == 0 && evalFirst) || (i > 0 && evalRest))
                    parts[i] = Evaluate(expr[i]);
                else
                    parts[i] = expr[i];
            }

            ExpressionValue evaluated;

            if (!attrs.HasFlag(SymbolAttributes.HoldAllComplete))
            {
                if (!attrs.HasFlag(SymbolAttributes.SequenceHold))
                {
                    parts = parts.SelectMany(FlattenHeads(_engine.System.Sequence)).ToArray();
                }

                if (attrs.HasFlag(SymbolAttributes.Flat))
                {
                    parts = parts.SelectMany(FlattenHeads(sym)).ToArray();
                }

                // TODO: if Listable ....
                // TODO: if Orderless ...

                evaluated = _engine.Expr(head, parts);

                foreach (var part in parts)
                {
                    var partSym = (part.AtomHead as SymbolValue)?.Symbol;
                    if (partSym == null) continue;

                    var upValued = _engine.PatternMatching.FindAndApplyRule(evaluated, partSym.UpValues);
                    if (upValued != null)
                        return (upValued, true);
                }

                foreach (var part in parts)
                {
                    var partSym = (part.AtomHead as SymbolValue)?.Symbol;
                    var value = partSym?.UpCode?.Invoke(_engine, evaluated);
                    if (value != null)
                    {
                        return (value, true);
                    }
                }
            }
            else
            {
                evaluated = _engine.Expr(head, parts);
            }

            // headsym is used for *value lookup
            // it's the leftmost atom in the expression
            // (head of head of .... till we get to atom)
            // THIS IS DIFFERENT FROM sym DECLARED AT THE START

            var headsym = (head.AtomHead as SymbolValue)?.Symbol;
            if (headsym == null)
                return (evaluated, false);

            var downValued = _engine.PatternMatching.FindAndApplyRule(evaluated, headsym.DownValues);
            if (downValued != null)
                return (downValued, true);

            var coded = headsym.DownCode?.Invoke(_engine, evaluated);
            if (coded != null)
            {
                return (coded, true);
            }

            return (evaluated, false);
        }

        private Func<Value, IEnumerable<Value>> FlattenHeads(Symbol h)
        {
            IEnumerable<Value> Helper(Value arg)
            {
                if (arg is ExpressionValue expr && expr.Head is SymbolValue sym && sym.Symbol == h)
                {
                    return expr.Parts.SelectMany(Helper);
                }

                return new[] {arg};
            }

            return Helper;
        }

        public Value Evaluate(Value v)
        {
            var iterationCount = 0;
            var needMore = true;
            while (needMore)
            {
                iterationCount++;

                if (iterationCount > _engine.IterationLimit)
                { 
                    // TODO: message
                }

                switch (v)
                {
                    case ExpressionValue expr:
                        (v, needMore) = EvaluateExpression(expr);
                        break;
                    case SymbolValue symValue:
                        (v, needMore) = EvaluateSymbol(symValue);
                        break;
                    default:
                        needMore = false;
                        break;
                }
            }

            return v;
        }

        private (Value, bool) EvaluateSymbol(SymbolValue symValue)
        {
            var sym = symValue.Symbol;

            var reg = _engine.PatternMatching.FindAndApplyRule(symValue, sym.OwnValues);
            if (reg != null)
                return (reg, true);


            var coded = sym.OwnCode?.Invoke(_engine, symValue);
            if (coded != null)
                return (coded, true);

            return (symValue, false);
        }
    }
}
