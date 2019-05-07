using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Osmium.Engine.Values;

namespace Osmium.Engine.Patterns.PatternClasses
{
    class ConditionPattern : Pattern
    {
        private readonly Pattern _pattern;
        private readonly Value _cond;

        public ConditionPattern(Pattern pattern, Value cond)
        {
            _pattern = pattern;
            _cond = cond;
        }

        public override IEnumerable<MatchResult> Match(Value expr, MatchContext context)
        {
            var engine = expr.Engine;
            return _pattern.Match(expr, context).Where(ApplyCondition);
        }

        public override IEnumerable<(MatchResult, IEnumerable<Value>)> MatchSequence(IEnumerable<Value> exprs, MatchContext context)
        {
            return _pattern.MatchSequence(exprs, context).Where(pair => ApplyCondition(pair.Item1));
        }

        private bool ApplyCondition(MatchResult res)
        {
            var engine = _cond.Engine;
            return engine
                .Evaluate(engine.PatternMatching.ToSingle(engine.PatternMatching.SymbolSubstitute(_cond, res.Context.Captures)))
                .Equals(engine.True);
        }
    }
}
