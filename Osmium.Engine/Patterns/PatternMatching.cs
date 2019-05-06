using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Osmium.Engine.Patterns.PatternClasses;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns
{
    public class PatternMatching
    {
        private OsmiumEngine _engine;

        public PatternMatching(OsmiumEngine engine)
        {
            _engine = engine;
        }

        public bool IsMatch(Value pattern, Value expr)
        {
            return Match(CompilePattern(pattern), expr).Any();
        }

        internal Pattern CompilePattern(Value pattern)
        {
            if (!(pattern is ExpressionValue expr))
                return new LiteralPattern(pattern);

            var head = expr.Head as Symbol;

            var system = _engine.System;

            if (head == null)
            {
            }
            else if (head == system.Blank)
            {
                if (expr.Count == 0)
                    return new BlankPattern();
                else if (expr.Count == 1)
                    return new BlankPattern(expr[0]);
                else
                {
                    // TODO: message
                }
            }
            else if (head == system.BlankSequence || head == system.BlankNullSequence)
            {
                if (expr.Count == 0)
                    return new BlankSequencePattern(head == system.BlankNullSequence);
                else if (expr.Count == 1)
                    return new BlankSequencePattern(head == system.BlankNullSequence, expr[0]);
                else
                {
                    // TODO: message
                }
            }
            else if (head == system.Pattern)
            {
                if (expr.Count == 2)
                {
                    var name = expr[0] as Symbol;
                    if (name == null)
                    {
                        // TODO: message
                    }
                    else
                    {
                        var pat = CompilePattern(expr[1]);
                        return new NamedPattern(name, pat);
                    }
                }
                else
                {
                    // TODO: message
                }
            }

            var headPat = CompilePattern(expr.Head);
            var partPats = expr.Parts.Select(CompilePattern).ToArray();

            if (headPat is LiteralPattern && partPats.All(p => p is LiteralPattern))
                return new LiteralPattern(pattern);

            return new ExprPattern(_engine, headPat, partPats);
        }

        internal static IEnumerable<MatchResult> Match(Pattern pattern, Value expr, MatchContext context = null)
        {
            context = context ?? MatchContext.Create();
            return pattern.Match(expr, context);
        }

        internal static IEnumerable<MatchResult> SequenceMatch(IEnumerable<Pattern> patterns, IEnumerable<Value> values, MatchContext ctx)
        {
            var patArray = patterns as Pattern[] ?? patterns.ToArray();
            var valArray = values as Value[] ?? values.ToArray();
            if (!patArray.Any())
                if (!valArray.Any())
                    yield return ctx.WithMatch();
                else
                    yield break;

            var curPat = patArray.First();
            var restPat = patArray.Skip(1);

            foreach (var (nctx, restExprs) in curPat.MatchSequence(valArray, ctx))
            {
                foreach (var nnctx in SequenceMatch(restPat, restExprs, nctx.Context))
                {
                    yield return nctx.Concat(nnctx);
                }
            }
        }

        internal Value FindAndApplyRule(Value original, List<(Pattern, Value)> rules)
        {
            foreach (var (pattern, value) in rules)
            {
                foreach (var matchResult in Match(pattern, original))
                {
                    return ToSingle(SymbolSubstitute(value, matchResult.Context.Captures));
                }
            }

            return null;
        }

        internal Value ToSingle(IEnumerable<Value> values)
        {
            var valArray = values as Value[] ?? values.ToArray();

            if (valArray.Length == 1)
                return valArray[0];

            return _engine.Expr(_engine.System.Sequence, valArray);
        }

        internal IEnumerable<Value> SymbolSubstitute(Value original, ImmutableDictionary<Symbol, IEnumerable<Value>> bound)
        {
            switch (original)
            {
                case ExpressionValue expr:
                    return _engine.Expr(
                        ToSingle(SymbolSubstitute(expr.Head, bound)),
                        expr.Parts.SelectMany(p => SymbolSubstitute(p, bound)).ToArray()
                    );
                case Symbol symValue:
                    if (bound.TryGetValue(symValue, out var replacement))
                        return replacement;
                    else
                        return new [] {original};
                default:
                    return new[] {original};
            }
        }
    }
}