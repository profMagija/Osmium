using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading;
using Osmium.Engine.Evaluation;
using Osmium.Engine.Parser;
using Osmium.Engine.Patterns;
using Osmium.Engine.Values;
using Osmium.Engine.Values.Numerics;

namespace Osmium.Engine
{
    public class OsmiumEngine
    {
        internal SystemSymbols System { get; }

        private SymbolTable _symTab;
        private Evaluator _evaluator;

        public PatternMatching PatternMatching { get; }

        public readonly NumberValue Zero;
        public readonly NumberValue One;
        public readonly NumberValue MinusOne;
        public readonly Symbol Null;

        public string Context { get; set; }
        public List<string> ContextPath { get; }
        public int IterationLimit { get; set; }

        public OsmiumEngine()
        {
            _symTab = new SymbolTable(this);
            _evaluator = new Evaluator(this);
            PatternMatching = new PatternMatching(this);
            System = new SystemSymbols(this);

            Context = "Global`";
            Zero = new IntegerValue(this, BigInteger.Zero);
            One = new IntegerValue(this, BigInteger.One);
            MinusOne = new IntegerValue(this, BigInteger.MinusOne);
            Null = System.Null;
            ContextPath = new List<string> {"System`"};
        }

        public Symbol GetSymbol(string context, string name)
        {
            return _symTab.GetSymbol(context + name);
        }

        public Symbol GetSymbol(string name)
        {
            if (name.Contains("`"))
            {
                if (name[0] == '`')
                {
                    // symbol name in current context

                    return _symTab.GetSymbol(Context + name.Substring(1));
                }
                
                // symbol name in specified context

                return _symTab.GetSymbol(name);
            }

            foreach (var context in ContextPath)
            {
                var sym = _symTab.TryGetSymbol(context + name);
                if (sym != null)
                    return sym;
            }

            return _symTab.GetSymbol(Context + name);
        }

        public ExpressionValue Expr(Value head, params Value[] values)
        {
            return new ExpressionValue(this, head, values);
        }

        public Symbol Sym(string name)
        {
            return GetSymbol(name);
        }

        public StringValue Str(string value)
        {
            return new StringValue(this, value);
        }

        public NumberValue Num(int value) => new IntegerValue(this, value);
        public NumberValue Num(double value) => new RealValue(this, value);

        public Value Evaluate(Value v)
        {
            var result = _evaluator.Evaluate(v);
            Line++;
            return result;
        }

        public int Line { get; set; } = 1;

        public Value Parse(string input)
        {
            return WolframLanguageParser.Parse(this, input);
        }

        public Value Evaluate(string input)
        {
            return Evaluate(Parse(input));
        }

        public bool IsMatch(Value pattern, Value what)
        {
            return PatternMatching.IsMatch(pattern, what);
        }
    }

    internal class SymbolTable
    {
        private readonly OsmiumEngine _engine;
        private readonly Dictionary<string, Symbol> _symbols;

        public SymbolTable(OsmiumEngine engine)
        {
            _engine = engine;
            _symbols = new Dictionary<string, Symbol>();
        }

        public Symbol GetSymbol(string key)
        {
            var sym = TryGetSymbol(key) ?? (_symbols[key] = new Symbol(_engine, key));

            return sym;
        }

        public Symbol TryGetSymbol(string key)
        {
            // TODO: limit symbol names to valid syntax

            if (!IsValidSymbolName(key))
                throw new ArgumentException($"Invalid symbol name: '{key}'", nameof(key));

            _symbols.TryGetValue(key, out var sym);

            return sym;
        }

        private static bool IsValidSymbolName(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            bool ValidChar(char c)
            {
                return char.IsLetterOrDigit(c) || c == '$';
            }

            bool ValidStartChar(char c)
            {
                return char.IsLetter(c) || c == '$';
            }

            var parts = key.Split('`');
            return parts.All(p => !string.IsNullOrEmpty(p) && ValidStartChar(p[0]) && p.All(ValidChar));
        }
    }
}
