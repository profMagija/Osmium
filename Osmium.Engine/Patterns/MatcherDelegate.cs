using System.Collections.Generic;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns
{
    internal delegate IEnumerable<MatchResult> MatcherDelegate(ExpressionValue pattern, Value expr, MatchContext context = null);

    internal delegate IEnumerable<(MatchResult, IEnumerable<Value>)> MatchSequenceDelegate(ExpressionValue pattern, IEnumerable<Value> exprs, MatchContext context);
}