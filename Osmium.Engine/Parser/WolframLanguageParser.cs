using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Osmium.Engine.Values;

namespace Osmium.Engine.Parser
{
    partial class WolframLanguageParser
    {
        public static Value Parse(OsmiumEngine engine, string input)
        {
            var lines = input.Split('\n');
            var lex = new WolframLanguageLexer(new AntlrInputStream(input));
            var parse = new WolframLanguageParser(new CommonTokenStream(lex, Lexer.DefaultTokenChannel));
            lex.Parser = parse;
            var listen = new ExprBuildListener(engine, parse);

            var errors = new List<string>();

            parse.AddErrorListener(new ErrorListener<IToken>((recognizer,  token,  line,  pos,  msg,  e) => { errors.Add($"Unexpected {token} at {line}:{pos}: {msg}"); }));
            var expr = parse.expr();
            ParseTreeWalker.Default.Walk(listen, expr);

            if (errors.Any())
            {
                foreach (var i in errors)
                    Console.WriteLine("err: {0}", i);
                return null;
            }

            return listen.GetValue();
        }

        private class ErrorListener<T> : IAntlrErrorListener<T>
        {
            private Action<IRecognizer, T, int, int, string, RecognitionException> _action;

            public ErrorListener(Action<IRecognizer, T, int, int, string, RecognitionException> action)
            {
                _action = action;
            }

            public void SyntaxError(IRecognizer recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                _action(recognizer, offendingSymbol, line, charPositionInLine, msg, e);
            }
        }
    }



    partial class WolframLanguageLexer
    {
        public WolframLanguageParser Parser;

        private CommonToken CommonToken(int type, string text)
        {
            int stop = CharIndex - 1;
            int start = text.Length == 0 ? stop : stop - text.Length + 1;
            return new CommonToken(this._tokenFactorySourcePair, type, DefaultTokenChannel, start, stop);
        }

        public override IToken NextToken()
        {

            var t = base.NextToken();

            if (t.Type == NEWLINE && Parser.IsExpectedToken(-1))
            {
                return CommonToken(-1, "<EOF>");
            }

            return t;
        }
    }
}
