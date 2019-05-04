using System;
using System.Collections.Generic;
using System.Text;
using Osmium.Engine.Patterns;
using Osmium.Engine.Patterns.PatternClasses;
using Osmium.Engine.Values;

namespace Osmium.Engine
{
    delegate (Value, bool)? OwnCodeCallback(OsmiumEngine engine, Value value);
    delegate (Value, bool)? ExprCodeCallback(OsmiumEngine engine, ExpressionValue value);

    [Flags]
    public enum SymbolAttributes
    {
        Orderless = 1 << 0,
        Flat = 1 << 1,
        OneIdentity = 1 << 2,
        Listable = 1 << 3,
        Constant = 1 << 4,
        NumericFunction = 1 << 5,
        Protected = 1 << 6,
        Locked = 1 << 7,
        ReadProtected = 1 << 8,
        HoldFirst = 1 << 9,
        HoldRest = 1 << 10,
        HoldAll = 1 << 11,
        HoldAllComplete = 1 << 12,
        NHoldFirst = 1 << 13,
        NHoldRest = 1 << 14,
        NHoldAll = 1 << 15,
        SequenceHold = 1 << 16,
        Temporary = 1 << 17,
        Stub = 1 << 18
    }

    public class Symbol
    {


        public string FullName { get; }

        internal List<(Pattern, Value)> OwnValues { get; } = new List<(Pattern, Value)>();
        internal OwnCodeCallback OwnCode { get; set; }
        internal List<(Pattern, Value)> UpValues { get; } = new List<(Pattern, Value)>();
        internal ExprCodeCallback UpCode { get; set; }
        internal List<(Pattern, Value)> DownValues { get; } = new List<(Pattern, Value)>();
        internal ExprCodeCallback DownCode { get; set; }

        internal SymbolAttributes Attributes { get; set; } = 0;

        private readonly OsmiumEngine _engine;

        public Symbol(OsmiumEngine engine, string fullName)
        {
            FullName = fullName;
            _engine = engine;
        }

        public Value ToValue()
        {
            return new SymbolValue(_engine, this);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
