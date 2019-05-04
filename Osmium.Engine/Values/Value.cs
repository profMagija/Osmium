namespace Osmium.Engine.Values
{
    public abstract class Value
    {
        protected Value(OsmiumEngine engine)
        {
            Engine = engine;
        }

        public abstract bool IsAtomic { get; }
        public abstract Value Head { get; }
        public abstract AtomicValue AtomHead { get; }

        protected OsmiumEngine Engine { get; }
    }
}