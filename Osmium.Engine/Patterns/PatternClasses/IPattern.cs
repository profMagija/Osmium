using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns.PatternClasses
{
    internal abstract class Pattern
    {
        public abstract IEnumerable<MatchResult> Match(Value expr, MatchContext context);
        public virtual IEnumerable<(MatchResult, IEnumerable<Value>)> MatchSequence(IEnumerable<Value> exprs, MatchContext context)
        {
            var enumerable = exprs as Value[] ?? exprs.ToArray();

            if (!enumerable.Any())
                return Enumerable.Empty<(MatchResult, IEnumerable<Value>)>();

            var curExpr = enumerable.First();
            var restExpr = enumerable.Skip(1).ToArray();

            return Match(curExpr, context).Select(ctx => (ctx, (IEnumerable<Value>)restExpr));
        }
    }

    internal class BlankPattern : Pattern
    {
        private readonly Value _head;

        public BlankPattern(Value head = null)
        {
            _head = head;
        }

        public override IEnumerable<MatchResult> Match(Value expr, MatchContext context)
        {
            if (_head == null || _head.Equals(expr.Head))
                return new[] {context.WithMatch(expr)};

            return new MatchResult[0];
        }
    }

    internal class BlankSequencePattern : Pattern
    {
        private readonly Value _head;
        private readonly bool _isNull;

        public BlankSequencePattern(bool isNull, Value head = null)
        {
            _head = head;
            _isNull = isNull;
        }

        public override IEnumerable<MatchResult> Match(Value expr, MatchContext context)
        {
            if (_head == null || _head.Equals(expr.Head))
                return new[] { context.WithMatch(expr) };

            return new MatchResult[0];
        }

        public override IEnumerable<(MatchResult, IEnumerable<Value>)> MatchSequence(IEnumerable<Value> exprs, MatchContext context)
        {
            var exprArray = exprs as Value[] ?? exprs.ToArray();

            if (_isNull)
                yield return (context.WithMatch(), exprArray);

            for (int i = 0; i < exprArray.Length; i++)
            {
                if (_head == null || _head.Equals(exprArray[i].Head))
                {
                    yield return (context.WithMatch(exprArray.Take(i + 1).ToArray()), exprArray.Skip(i + 1));
                }
                else
                    yield break;
            }
        }
    }

    internal class LiteralPattern : Pattern
    {
        private Value _literal;

        public LiteralPattern(Value literal)
        {
            _literal = literal;
        }

        public override IEnumerable<MatchResult> Match(Value expr, MatchContext context)
        {
            return _literal.Equals(expr) ? new[] {context.WithMatch(expr)} : new MatchResult[0];
        }
    }

    internal class NamedPattern : Pattern
    {
        private readonly Symbol _name;
        private readonly Pattern _pattern;

        public NamedPattern(Symbol name, Pattern pattern)
        {
            _name = name;
            _pattern = pattern;
        }

        public override IEnumerable<MatchResult> Match(Value expr, MatchContext context)
        {
            return _pattern.Match(expr, context).Select(ctx => ctx.Context.WithCapture(_name, ctx.SequenceMatch).WithMatch(ctx.SequenceMatch));
        }

        public override IEnumerable<(MatchResult, IEnumerable<Value>)> MatchSequence(IEnumerable<Value> exprs, MatchContext context)
        {
            return from pair in _pattern.MatchSequence(exprs, context)
                let ctx = pair.Item1
                let rest = pair.Item2
                select (ctx.Context.WithCapture(_name, ctx.SequenceMatch).WithMatch(ctx.SequenceMatch), rest);
        }
    }

    internal class ExprPattern : Pattern
    {
        private readonly OsmiumEngine _engine;
        private readonly Pattern _head;
        private readonly Pattern[] _parts;

        public ExprPattern(OsmiumEngine engine, Pattern head, Pattern[] parts)
        {
            _engine = engine;
            _head = head;
            _parts = parts;
        }

        public override IEnumerable<MatchResult> Match(Value v, MatchContext context)
        {
            if (!(v is ExpressionValue expr))
                return new MatchResult[0];

            return _head.Match(expr.Head, context)
                .SelectMany(res => PatternMatching.SequenceMatch(_parts, expr.Parts, res.Context)
                    .Select(partRes => partRes.Context.WithMatch(_engine.Expr(
                        _engine.PatternMatching.ToSingle(res.SequenceMatch),
                        partRes.SequenceMatch
                    ))));
        }
    }
}
