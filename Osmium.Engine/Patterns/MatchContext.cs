using System.Collections.Generic;
using System.Collections.Immutable;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns
{
    internal class MatchContext
    {
        public ImmutableDictionary<Symbol, IEnumerable<Value>> Captures { get; }

        private MatchContext(ImmutableDictionary<Symbol, IEnumerable<Value>> captures)
        {
            Captures = captures;
        }

        public MatchResult WithMatch(params Value[] expr)
        {
            return new MatchResult(this, expr);
        }

        public static MatchContext Create()
        {
            return new MatchContext(ImmutableDictionary<Symbol, IEnumerable<Value>>.Empty);
        }

        public MatchContext WithCapture(Symbol name, IEnumerable<Value> match)
        {
            return new MatchContext(Captures.Add(name, match));
        }
    }
}