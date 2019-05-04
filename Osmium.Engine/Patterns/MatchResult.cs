using System.Collections.Generic;
using System.Linq;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns
{
    internal class MatchResult
    {
        public MatchContext Context { get; }
        public Value[] SequenceMatch { get; }

        internal MatchResult(MatchContext context, Value[] sequenceMatch)
        {
            Context = context;
            SequenceMatch = sequenceMatch;
        }

        public MatchResult Concat(MatchResult other)
        {
            return new MatchResult(other.Context, SequenceMatch.Concat(other.SequenceMatch).ToArray());
        }
    }
}