using System;

namespace Osmium.Engine.Values
{
    public abstract class Value : IEquatable<Value>
    {
        protected Value(OsmiumEngine engine)
        {
            Engine = engine;
        }

        public abstract bool IsAtomic { get; }
        public abstract Value Head { get; }
        public abstract AtomicValue AtomHead { get; }

        protected internal OsmiumEngine Engine { get; }

        bool IEquatable<Value>.Equals(Value other)
        {
            return Equals((object) other);
        }
    }
}