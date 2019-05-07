using System.Collections.Generic;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns.PatternClasses
{
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
}