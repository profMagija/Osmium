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
}