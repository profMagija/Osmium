using System.Collections.Generic;
using System.Linq;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns.PatternClasses
{
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