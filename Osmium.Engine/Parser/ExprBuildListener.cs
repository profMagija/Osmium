using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Osmium.Engine.Values;
using Osmium.Engine.Values.Numerics;

namespace Osmium.Engine.Parser
{
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
            var count = CsExprCount(context.csexpr());
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

        private int CsExprCount(WolframLanguageParser.CsexprContext context)
        {
            return context.expr() == null ? 0 : context.optexpr().Length + 1;
        }

        public override void ExitPatternTest(WolframLanguageParser.PatternTestContext context)
        {
            var pts = context.messageName().Length;

            if (pts == 1)
                return;

            Construct(_engine.System.PatternTest, 2);
        }

        public override void ExitBracketedPartPlain(WolframLanguageParser.BracketedPartPlainContext context)
        {
            MakePart(CsExprCount(context.csexpr()) + 1);
        }

        public override void ExitBracketedFunctionApplication(WolframLanguageParser.BracketedFunctionApplicationContext context)
        {
            var argcount = CsExprCount(context.csexpr());
            var parts = Pop(argcount);
            var head = Pop(1)[0];
            Push(_engine.Expr(head, parts));
        }

        public override void ExitBracketedPartFancy(WolframLanguageParser.BracketedPartFancyContext context)
        {
            MakePart(CsExprCount(context.csexpr()) + 1);
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

        public override void ExitPrefixPlusMinus(WolframLanguageParser.PrefixPlusMinusContext context)
        {
            switch (context.Start.Text)
            {
                case "-":
                    Push(_engine.Expr(_engine.System.Times.ToValue(),
                            _engine.MinusOne,
                            Pop()
                        )
                    );
                    break;
                case "+":
                    break;
                case "\xb1":
                    Construct(_engine.System.PlusMinus, 1);
                    break;
                case "\U00002213":
                    Construct(_engine.System.MinusPlus, 1);
                    break;
            }
        }

        public override void ExitDivisionRule(WolframLanguageParser.DivisionRuleContext context)
        {
            var args = Pop(2);
            // Times[a, Power[b, -1]]
            Push(_engine.Expr(_engine.System.Times.ToValue(),
                    args[0],
                    _engine.Expr(_engine.System.Power.ToValue(),
                        args[1],
                        _engine.MinusOne
                    )
                )
            );
        }

        public override void ExitTimes(WolframLanguageParser.TimesContext context)
        {
            var divs = context.division().Length;

            if (divs == 1)
                return;

            Construct(_engine.System.Times, divs);
        }

        public override void ExitPlusMinus(WolframLanguageParser.PlusMinusContext context)
        {
            var parts = context.plusMinusPart().Length + 1;

            if (parts == 1)
                return;

            Construct(_engine.System.Plus, parts);
        }

        public override void ExitNegateRule(WolframLanguageParser.NegateRuleContext context)
        {
            var v = Pop();
            Push(_engine.Expr(_engine.System.Times.ToValue(), _engine.MinusOne, v));
        }

        public override void ExitComparison(WolframLanguageParser.ComparisonContext context)
        {
            var parts = context.plusMinus().Length;
            if (parts == 1)
                return;

            var syms = context.compOperator().Select(x => GetSymbolForOp(x.GetText())).ToArray();

            if (syms.All(x => x == syms[0]))
            {
                Construct(syms[0], parts);
            }
            else
            {
                var partValues = Pop(parts);

                var l = new Value[2 * parts - 1];
                l[0] = partValues[0];

                for (var i = 1; i < parts; i++)
                {
                    l[2 * i - 1] = syms[i - 1].ToValue();
                    l[2 * i] = partValues[i];
                }

                Push(_engine.Expr(_engine.System.Inequality.ToValue(), l));
            }
        }

        public override void ExitSameQRule(WolframLanguageParser.SameQRuleContext context)
        {
            var parts = context.comparison().Length;
            Construct(_engine.System.SameQ, parts);
        }

        public override void ExitUnsameQRule(WolframLanguageParser.UnsameQRuleContext context)
        {
            Construct(_engine.System.UnsameQ, 2);
        }

        public override void ExitNotRule(WolframLanguageParser.NotRuleContext context)
        {
            Construct(_engine.System.Not, 1);
        }

        public override void ExitAnds(WolframLanguageParser.AndsContext context)
        {
            var parts = context.nots().Length;
            if (parts == 1)
                return;

            Construct(_engine.System.And, parts);
        }

        public override void ExitOrs(WolframLanguageParser.OrsContext context)
        {
            var parts = context.ands().Length;
            if (parts == 1)
                return;

            Construct(_engine.System.Or, parts);
        }

        public override void ExitRepeatedRule(WolframLanguageParser.RepeatedRuleContext context)
        {
            Construct(_engine.System.Repeated, 1);
        }

        public override void ExitRepeatedNullRule(WolframLanguageParser.RepeatedNullRuleContext context)
        {
            Construct(_engine.System.RepeatedNull, 1);
        }

        public override void ExitAlternatives(WolframLanguageParser.AlternativesContext context)
        {
            var parts = context.repeated().Length;
            if (parts == 1)
                return;

            Construct(_engine.System.Alternatives, parts);
        }

        public override void ExitNamedPattern(WolframLanguageParser.NamedPatternContext context)
        {
            var v = Pop();
            var sym = CreateSymbol(context.SYMBOL().GetText());
            Push(_engine.Expr(_engine.System.Pattern.ToValue(),
                sym, v
            ));
        }

        public override void ExitOptionalWithDefault(WolframLanguageParser.OptionalWithDefaultContext context)
        {
            var v = Pop();
            var sym = CreateBlank(context.BLANKFORM().GetText());

            Push(_engine.Expr(_engine.System.Optional.ToValue(),
                sym, v
            ));
        }

        public override void ExitStringExpr(WolframLanguageParser.StringExprContext context)
        {
            var parts = context.pattern().Length;
            if (parts == 1)
                return;

            Construct(_engine.System.StringExpression, parts);
        }

        public override void ExitConditionRule(WolframLanguageParser.ConditionRuleContext context)
        {
            Construct(_engine.System.Condition, 2);
        }

        public override void ExitTwoWayRuleRule(WolframLanguageParser.TwoWayRuleRuleContext context)
        {
            Construct(_engine.System.TwoWayRule, 2);
        }

        public override void ExitRuleRule(WolframLanguageParser.RuleRuleContext context)
        {
            Construct(_engine.System.Rule, 2);
        }

        public override void ExitDelayedRuleRule(WolframLanguageParser.DelayedRuleRuleContext context)
        {
            Construct(_engine.System.DelayedRule, 2);
        }

        public override void ExitReplaceAllRule(WolframLanguageParser.ReplaceAllRuleContext context)
        {
            Construct(_engine.System.ReplaceAll, 2);
        }

        public override void ExitReplaceRepeatedRule(WolframLanguageParser.ReplaceRepeatedRuleContext context)
        {
            Construct(_engine.System.ReplaceRepeated, 2);
        }

        public override void ExitAugmentedSet(WolframLanguageParser.AugmentedSetContext context)
        {
            Construct(GetSymbolForOp(context.augSetOp().GetText()), 2);
        }

        public override void ExitFunctionRule(WolframLanguageParser.FunctionRuleContext context)
        {
            Construct(_engine.System.Function, 1);
        }

        public override void ExitSetSimple(WolframLanguageParser.SetSimpleContext context)
        {
            Construct(GetSymbolForOp(context.setOp().GetText()), 2);
        }

        public override void ExitTagSet(WolframLanguageParser.TagSetContext context)
        {
            Symbol s;
            switch (context.setOp2().GetText())
            {
                case "=":
                    s = _engine.System.TagSet;
                    break;
                case ":=":
                    s = _engine.System.TagSetDelayed;
                    break;
                default:
                    Debug.Fail("should not happen");
                    s = null;
                    break;
            }

            var sym = CreateSymbol(context.SYMBOL().GetText());
            var parts = Pop(2);

            Push(_engine.Expr(s.ToValue(), sym, parts[0], parts[1]));
        }

        public override void ExitClearSimple(WolframLanguageParser.ClearSimpleContext context)
        {
            Construct(_engine.System.Unset, 1);
        }


        public override void ExitTagClear(WolframLanguageParser.TagClearContext context)
        {
            var sym = CreateSymbol(context.SYMBOL().GetText());
            var val = Pop();
            Push(_engine.Expr(_engine.System.TagUnset.ToValue(), sym, val));
        }

        public override void ExitLambda(WolframLanguageParser.LambdaContext context)
        {
            Construct(_engine.System.Function, 2);
        }

        public override void ExitPutRule(WolframLanguageParser.PutRuleContext context)
        {
            var pf = context.PUTFORM();
            if (pf != null)
            {
                var fn = pf.GetText().Substring(2).TrimStart(' ', '\t', '\v', '\f', '\n');
                var expr = Pop();
                Push(_engine.Expr(_engine.System.Put.ToValue(), expr, _engine.Str(fn)));
            }
            else
            {
                var fn = context.APPENDFORM().GetText().Substring(2).TrimStart(' ', '\t', '\v', '\f', '\n');
                var expr = Pop();
                Push(_engine.Expr(_engine.System.PutAppend.ToValue(), expr, _engine.Str(fn)));
            }
        }

        public override void ExitPutExprRule(WolframLanguageParser.PutExprRuleContext context)
        {
            Construct(GetSymbolForOp(context.putOp().GetText()), 2);
        }

        public override void ExitCompoundExpr(WolframLanguageParser.CompoundExprContext context)
        {
            var count = context.putopt().Length + 1;

            if (count == 1)
                return;

            Construct(_engine.System.CompoundExpression, count);
        }

        public override void ExitPutopt(WolframLanguageParser.PutoptContext context)
        {
            if (context.put() == null)
            {
                Push(_engine.System.Null.ToValue());
            }
        }

        private Symbol GetSymbolForOp(string op)
        {
            var system = _engine.System;
            switch (op)
            {
                case "/@": return system.Map;
                case "//@": return system.MapAll;
                case "@@": return system.Apply;
                case "\U0000f3c8": return system.Conjugate;
                case "\U0000f3c7": return system.Transpose;
                case "\U0000f3c9":
                case "\U0000f3ce": return system.ConjugateTranspose;
                case "==": return system.Equal;
                case "!=": return system.Unequal;
                case ">": return system.Greater;
                case "<": return system.Less;
                case ">=": return system.GreaterEqual;
                case "<=": return system.LessEqual;
                case "===": return system.SameQ;
                case "=!=": return system.UnsameQ;
                case "+=": return system.AddTo;
                case "-=": return system.SubtractFrom;
                case "*=": return system.TimesBy;
                case "/=": return system.DivideBy;
                case "=": return system.Set;
                case ":=": return system.SetDelayed;
                case "^=": return system.UpSet;
                case "^:=": return system.UpSetDelayed;
                case ">>": return system.Put;
                case ">>>": return system.PutAppend;
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
            var data = text.Substring(1, text.Length - 2);

            var sb = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '\\')
                {
                    switch (data[i + 1])
                    {
                        case '\\':
                        case ' ':
                        case '"':
                            sb.Append(data[i + 1]);
                            i++;
                            break;
                        case 'f':
                            sb.Append('\f');
                            i++;
                            break;
                        case 'b':
                            sb.Append('\b');
                            i++;
                            break;
                        case 't':
                            sb.Append('\t');
                            i++;
                            break;
                        case 'n':
                            sb.Append('\n');
                            i++;
                            break;
                        case 'r':
                            sb.Append('\r');
                            i++;
                            break;
                        default:
                            sb.Append('\\');
                            break;
                    }
                }
                else
                {
                    sb.Append(data[i]);
                }
            }

            return new StringValue(_engine, sb.ToString());
        }

        private Value CreateNumber(string numText)
        {
            if (numText.Contains("."))
                return new RealValue(_engine, double.Parse(numText));
            return new IntegerValue(_engine, int.Parse(numText));
        }
    }
}