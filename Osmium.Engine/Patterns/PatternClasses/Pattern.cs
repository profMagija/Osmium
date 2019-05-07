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
}
