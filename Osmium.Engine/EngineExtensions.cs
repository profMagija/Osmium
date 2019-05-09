using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Osmium.Engine.Values;

namespace Osmium.Engine
{
    public static class EngineExtensions
    {
        public static void Set(this OsmiumEngine engine, Value lhs, Value rhs)
        {
            engine.Evaluate(engine.Expr(engine.System.Set,
                lhs,
                rhs
            ));
        }

        public static void SetDelayed(this OsmiumEngine engine, Value lhs, Value rhs)
        {
            engine.Evaluate(engine.Expr(engine.System.SetDelayed,
                lhs,
                rhs
            ));
        }

        public static ExpressionValue List(this OsmiumEngine engine, params Value[] values)
        {
            return engine.Expr(engine.System.List, values);
        }
    }
}
