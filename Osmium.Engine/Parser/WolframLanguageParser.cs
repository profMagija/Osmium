using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Osmium.Engine.Values;
using Osmium.Engine.Values.Numerics;

namespace Osmium.Engine.Parser
{
    partial class WolframLanguageParser
    {
        public static Value Parse(OsmiumEngine engine, string input)
        {
            var lex = new WolframLanguageLexer(new AntlrInputStream(input));
            var parse = new WolframLanguageParser(new CommonTokenStream(lex, Lexer.DefaultTokenChannel));
            var listen = new ExprBuildListener(engine, parse);
            parse.AddParseListener(listen);
            parse.expr();
            return listen.GetValue();
        }
    }

    class ExprBuildListener : WolframLanguageBaseListener
    {
        private List<Value> _valueStack = new List<Value>();

        public Value GetValue()
        {
            return _valueStack.Single();
        }

        private Value[] Pop(int count)
        {
            var start = _valueStack.Count - count;
            var rv = _valueStack.GetRange(start, count).ToArray();
            _valueStack.RemoveRange(start, count);
            return rv;
        }

        private Value Pop()
        {
            var rv = _valueStack[_valueStack.Count - 1];
            _valueStack.RemoveAt(_valueStack.Count - 1);
            return rv;
        }

        private void Push(Value v)
        {
            _valueStack.Add(v);
        }

        private readonly OsmiumEngine _engine;
        private readonly WolframLanguageParser _parse;

        public ExprBuildListener(OsmiumEngine engine, WolframLanguageParser parse)
        {
            _engine = engine;
            _parse = parse;
        }

        public override void ExitAtomBlank(WolframLanguageParser.AtomBlankContext context)
        {
            Push(CreateBlank(context.BLANKFORM().GetText()));
        }

        public override void ExitAtomGet(WolframLanguageParser.AtomGetContext context)
        {
            Push(
                _engine.Expr(_engine.System.Get.ToValue(),
                    _engine.Str(context.GETFORM().GetText().Substring(2).TrimStart(' ', '\n', '\t', '\v', '\f'))
                )
            );
        }

        public override void ExitAtomNumber(WolframLanguageParser.AtomNumberContext context)
        {
            Push(CreateNumber(context.NUMBER().GetText()));
        }

        public override void ExitAtomMatchfix(WolframLanguageParser.AtomMatchfixContext context)
        {
            var count = context.csexpr().optexpr().Length;
            var left = context.XLEFT().GetText();
            var right = context.XRIGHT().GetText();
            switch (left)
            {
                case "{":
                    if (right != "}")
                        _parse.NotifyErrorListeners($"Expression \"{context.GetText()}\" has no enclosing \"}}\"");
                    else
                        Push(
                            _engine.Expr(_engine.System.List.ToValue(),
                                Pop(count)
                            )
                        );
                    break;
                case "<|":
                    if (right != "}")
                        _parse.NotifyErrorListeners($"Expression \"{context.GetText()}\" has no enclosing \"}}\"");
                    else
                        Push(
                            _engine.Expr(_engine.System.Association.ToValue(),
                                Pop(count)
                            )
                        );
                    break;
                default:
                    Debug.Fail("Should not happen");
                    break;
            }
        }

        public override void ExitAtomCharString(WolframLanguageParser.AtomCharStringContext context)
        {
            Push(CreateString(context.GetText()));
        }

        public override void ExitAtomGetExpr(WolframLanguageParser.AtomGetExprContext context)
        {
            Push(
                _engine.Expr(_engine.System.Get.ToValue(),
                    Pop(1)
                )
            );
        }

        public override void ExitAtomSymbol(WolframLanguageParser.AtomSymbolContext context)
        {
            Push(CreateSymbol(context.SYMBOL().GetText()));
        }

        public override void ExitAtomOut(WolframLanguageParser.AtomOutContext context)
        {

            var text = context.OUTFORM().GetText();
            if (text.Length == 1)
                Push(_engine.Expr(_engine.System.Out.ToValue()));
            else if (text[1] == '%')
                Push(_engine.Expr(_engine.System.Out.ToValue(), new IntegerValue(_engine, -text.Length)));
            else
                Push(_engine.Expr(_engine.System.Out.ToValue(), CreateNumber(text.Substring(1))));
        }

        public override void ExitAtomSlot(WolframLanguageParser.AtomSlotContext context)
        {
            var text = context.GetText();

            if (text.Length == 1)
                Push(_engine.Expr(_engine.System.Slot.ToValue(), _engine.One));

            else if (text[1] == '#')
                Push(_engine.Expr(_engine.System.SlotSequence.ToValue(),
                    text.Length == 2 ? _engine.One : CreateNumber(text.Substring(2)))
                );
            else
                Push(_engine.Expr(_engine.System.Slot.ToValue(),
                    char.IsDigit(text[1]) ? CreateNumber(text.Substring(1)) : _engine.Str(text.Substring(1)))
                );
        }

        private Value CreateBlank(string text)
        {
            // xxxx__yyyyy
            //     ^ ^
            //     i j
            var i = text.IndexOf('_');
            var j = text.LastIndexOf('_') + 1;

            var isNamed = i != 0;
            var hasHead = j < text.Length && text[j] != '.';
            var isOpt = j < text.Length && text[j] == '.';

            Symbol sym;
            switch (j - i)
            {
                case 1:
                    sym = _engine.System.Blank;
                    break;
                case 2:
                    sym = _engine.System.BlankSequence;
                    break;
                case 3:
                    sym = _engine.System.BlankNullSequence;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var val = _engine.Expr(sym.ToValue(), hasHead ? new Value[] { CreateSymbol(text.Substring(j)) } : new Value[0]);

            if (isNamed)
            {
                val = _engine.Expr(_engine.System.Pattern.ToValue(),
                    CreateSymbol(text.Substring(0, i)),
                    val
                );
            }

            if (isOpt)
            {
                val = _engine.Expr(_engine.System.Optional.ToValue(), val);
            }

            return val;
        }

        public override void ExitMessageName(WolframLanguageParser.MessageNameContext context)
        {
            var parts = context.messageNamePart();

            if (parts.Length == 0)
                return;

            Construct(_engine.System.MessageName, parts.Length + 1);
        }

        public override void ExitMessageNamePart(WolframLanguageParser.MessageNamePartContext context)
        {
            var atomic = context.atomic();

            if (atomic.Start != atomic.Stop)
                return;
            
            Pop(1);
            Push(_engine.Str(atomic.Start.Text));
        }

        public override void ExitBracketedPartPlain(WolframLanguageParser.BracketedPartPlainContext context)
        {
            MakePart(context.csexpr().optexpr().Length + 1);
        }

        public override void ExitBracketedFunctionApplication(WolframLanguageParser.BracketedFunctionApplicationContext context)
        {
            var argcount = context.csexpr().optexpr().Length;
            var parts = Pop(argcount);
            var head = Pop(1)[0];
            Push(_engine.Expr(head, parts));
        }

        public override void ExitBracketedPartFancy(WolframLanguageParser.BracketedPartFancyContext context)
        {
            MakePart(context.csexpr().optexpr().Length + 1);
        }

        private void MakePart(int count)
        {
            Construct(_engine.System.Part, count);
        }

        public override void ExitOptexpr(WolframLanguageParser.OptexprContext context)
        {
            if (context.expr() == null)
                Push(_engine.System.Null.ToValue());
        }

        private void Construct(Symbol sym, int count)
        {
            Push(
                _engine.Expr(sym.ToValue(), Pop(count))
            );
        }

        public override void ExitPreDecrement(WolframLanguageParser.PreDecrementContext context)
        {
            Construct(_engine.System.PreDecrement, 1);
        }

        public override void ExitPreIncrement(WolframLanguageParser.PreIncrementContext context)
        {
            Construct(_engine.System.PreIncrement, 1);
        }

        public override void ExitDecrement(WolframLanguageParser.DecrementContext context)
        {
            Construct(_engine.System.Decrement, 1);
        }

        public override void ExitIncrement(WolframLanguageParser.IncrementContext context)
        {
            Construct(_engine.System.Increment, 1);
        }

        public override void ExitCompositionRule(WolframLanguageParser.CompositionRuleContext context)
        {
            Construct(_engine.System.Composition, context.prefixIncDec().Length);
        }

        public override void ExitRightCompositionRule(WolframLanguageParser.RightCompositionRuleContext context)
        {
            Construct(_engine.System.RightComposition, context.composition().Length);
        }

        public override void ExitApplicationRule(WolframLanguageParser.ApplicationRuleContext context)
        {
            var args = Pop(2);
            Push(_engine.Expr(args[0], args[1]));
        }

        public override void ExitInfixApplication(WolframLanguageParser.InfixApplicationContext context)
        {
            var args = Pop(3);
            Push(_engine.Expr(args[1], args[0], args[2]));
        }

        public override void ExitMapApplyRule(WolframLanguageParser.MapApplyRuleContext context)
        {
            var text = context.mapApplyOperator().GetText();
            if (text == "@@@")
            {
                Push(_engine.Expr(_engine.System.List.ToValue(), _engine.Num(1)));
                Construct(_engine.System.Apply, 3);
            }
            else
            {
                Construct(GetSymbolForOp(text), 2);
            }
        }

        public override void ExitFactorialRule(WolframLanguageParser.FactorialRuleContext context)
        {
            Construct(_engine.System.Factorial, 1);
        }

        public override void ExitFactorial2Rule(WolframLanguageParser.Factorial2RuleContext context)
        {
            Construct(_engine.System.Factorial2, 1);
        }

        public override void ExitConjTransposeRule(WolframLanguageParser.ConjTransposeRuleContext context)
        {
            Construct(GetSymbolForOp(context.conjTranspOp().GetText()), 1);
        }

        public override void ExitDerivative(WolframLanguageParser.DerivativeContext context)
        {
            var dnum = context.DERIV().Length;

            if (dnum == 0)
                return;

            var der = _engine.Expr(_engine.System.Derivative.ToValue(), _engine.Num(dnum));
            Push(_engine.Expr(der, Pop()));
        }

        public override void ExitStringJoin(WolframLanguageParser.StringJoinContext context)
        {
            var count = context.derivative().Length;

            if (count == 1)
                return;

            Construct(_engine.System.StringJoin, count);
        }

        // TODO: prefixpm ...

        private Symbol GetSymbolForOp(string op)
        {
            switch (op)
            {
                case "/@": return _engine.System.Map;
                case "//@": return _engine.System.MapAll;
                case "@@": return _engine.System.Apply;
                case "\uf3c8":
                    return _engine.System.Conjugate;
                case "\uf3c7":
                    return _engine.System.Transpose;
                case "\uf3c9":
                case "\uf3ce":
                    return _engine.System.ConjugateTranspose;
                default:
                    Debug.Fail("Should not happen");
                    return null;
            }
        }

        private Value CreateSymbol(string text)
        {
            return _engine.Sym(text);
        }

        private Value CreateString(string text)
        {
            throw new NotImplementedException();
        }

        private Value CreateNumber(string startText)
        {
            throw new NotImplementedException();
        }
    }
}
