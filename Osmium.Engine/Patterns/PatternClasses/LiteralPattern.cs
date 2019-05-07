using System.Collections.Generic;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns.PatternClasses
{
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
}