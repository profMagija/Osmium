using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Osmium.Engine.Values
{
    public sealed class ExpressionValue : Value, IReadOnlyList<Value>
    {
        public IReadOnlyList<Value> Parts { get; }

        public override bool IsAtomic => false;
        public override Value Head { get; }
        public override AtomicValue AtomHead { get; }
        public Symbol SymbolHead => AtomHead as Symbol;

        public ExpressionValue(OsmiumEngine engine, Value head, params Value[] parts) : base(engine)
        {
            Head = head ?? throw new ArgumentNullException(nameof(head));
            Parts = parts ?? throw new ArgumentNullException(nameof(parts));

            AtomHead = Head.AtomHead;
        }

        private bool Equals(ExpressionValue other)
        {
            return Parts.Count == other.Parts.Count && Parts.Zip(other.Parts, Equals).All(b => b) && Head.Equals(other.Head);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ExpressionValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Parts.GetHashCode() * 397) ^ Head.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Head + "[" + string.Join(", ", Parts.Select(x => x.ToString())) + "]";
        }

        #region IList implementation

        public IEnumerator<Value> GetEnumerator()
        {
            return Parts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Parts).GetEnumerator();
        }

        public int Count => Parts.Count;

        public Value this[int index] => Parts[index];

        #endregion

    }
}