using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Osmium.Engine.Values;

namespace Osmium.Engine.Parser
{
    partial class WolframLanguageParser
    {
        public static Value Parse(OsmiumEngine engine, string input)
        {
            var lex = new WolframLanguageLexer(new AntlrInputStream(input));
            var parse = new WolframLanguageParser(new CommonTokenStream(lex, Lexer.DefaultTokenChannel));
            var listen = new ExprBuildListener(engine, parse);
            var expr = parse.expr();
            ParseTreeWalker.Default.Walk(listen, expr);
            return listen.GetValue();
        }
    }
}
