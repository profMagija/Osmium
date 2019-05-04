namespace Osmium.Engine.Values
{
    public abstract class AtomicValue : Value
    {
        public override bool IsAtomic => true;
        public override AtomicValue AtomHead => this;

        protected AtomicValue(OsmiumEngine engine) : base(engine)
        {
        }
    }
}