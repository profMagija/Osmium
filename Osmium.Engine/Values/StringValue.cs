using System;
using System.Collections.Generic;
using System.Text;

namespace Osmium.Engine.Values
{
    public sealed class StringValue : AtomicValue
    {
        public override Value Head => Engine.System.String;
        public string Value { get; }

        public StringValue(OsmiumEngine engine, string value) : base(engine)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private bool Equals(StringValue other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is StringValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return  "\"" + Value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }
    }
}
