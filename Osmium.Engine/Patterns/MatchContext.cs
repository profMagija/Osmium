using System.Collections.Generic;
using System.Collections.Immutable;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns
{
    internal class MatchContext
    {
        public ImmutableDictionary<Symbol, Value[]> Captures { get; }

        private MatchContext(ImmutableDictionary<Symbol, Value[]> captures)
        {
            Captures = captures;
        }

        public MatchResult WithMatch(params Value[] expr)
        {
            return new MatchResult(this, expr);
        }

        public static MatchContext Create()
        {
            return new MatchContext(ImmutableDictionary<Symbol, Value[]>.Empty);
        }

        public MatchContext WithCapture(Symbol name, Value[] match)
        {
            return new MatchContext(Captures.Add(name, match));
        }
    }
}