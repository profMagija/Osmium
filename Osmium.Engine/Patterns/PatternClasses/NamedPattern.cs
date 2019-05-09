using System;
using System.Collections.Generic;
using System.Linq;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns.PatternClasses
{
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
            return _pattern.Match(expr, context).Select(FilterMatch).Where(x => x != null);
        }

        private MatchResult FilterMatch(MatchResult ctx)
        {
            if (ctx.Context.Captures.TryGetValue(_name, out var existing))
            {
                return existing.SequenceEqual(ctx.SequenceMatch) ? ctx : null;
            }

            return ctx.Context.WithCapture(_name, ctx.SequenceMatch).WithMatch(ctx.SequenceMatch);
        }

        public override IEnumerable<(MatchResult, IEnumerable<Value>)> MatchSequence(IEnumerable<Value> exprs, MatchContext context)
        {
            return _pattern.MatchSequence(exprs, context).Select(x => (FilterMatch(x.Item1), x.Item2)).Where(x => x.Item1 != null);
        }
    }
}