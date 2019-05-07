using System.Collections.Generic;
using System.Linq;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns.PatternClasses
{
    internal class BlankSequencePattern : Pattern
    {
        private readonly Value _head;
        private readonly bool _isNull;

        public BlankSequencePattern(bool isNull, Value head = null)
        {
            _head = head;
            _isNull = isNull;
        }

        public override IEnumerable<MatchResult> Match(Value expr, MatchContext context)
        {
            if (_head == null || _head.Equals(expr.Head))
                return new[] { context.WithMatch(expr) };

            return new MatchResult[0];
        }

        public override IEnumerable<(MatchResult, IEnumerable<Value>)> MatchSequence(IEnumerable<Value> exprs, MatchContext context)
        {
            var exprArray = exprs as Value[] ?? exprs.ToArray();

            if (_isNull)
                yield return (context.WithMatch(), exprArray);

            for (int i = 0; i < exprArray.Length; i++)
            {
                if (_head == null || _head.Equals(exprArray[i].Head))
                {
                    yield return (context.WithMatch(exprArray.Take(i + 1).ToArray()), exprArray.Skip(i + 1));
                }
                else
                    yield break;
            }
        }
    }
}