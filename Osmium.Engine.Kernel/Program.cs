using System;
using System.Linq;
using Osmium.Engine.Values;

namespace Osmium.Engine.Kernel
{
    class Program
    {
        static void Main(string[] args)
        {
            var prompt = true;

            if (prompt)
            {
                Console.WriteLine("Osmium Kernel");
            }

            var engine = new OsmiumEngine();

            while (true)
            {
                if (!engine.System.SLine.HasOwnValue())
                {
                    // $Line = 1
                    engine.Set(engine.System.SLine, engine.One);
                }

                if (prompt)
                {
                    Console.WriteLine();
                    Console.Write("In[{0}]:= ", engine.System.SLine.GetValue());
                }

                var input = Console.ReadLine();

                input = (ApplyIfDefined(engine, engine.System.SPreRead, engine.Str(input)) as StringValue)?.Value ?? "";

                var parsed = engine.Parse(input);

                if (parsed == null)
                {
                    continue;
                }

                AddDownValue(engine, engine.System.InString, engine.Str(input), engine.System.SLine);
                AddDownValue(engine, engine.System.In, parsed, engine.System.SLine);

                var output = engine.Evaluate(engine.System.SPre.HasOwnValue() ? engine.Expr(engine.System.SPre, parsed) : parsed);

                output = ApplyIfDefined(engine, engine.System.SPost, output);

                AddDownValue(engine, engine.System.Out, output);

                engine.SetDelayed(
                    engine.Expr(engine.System.MessageList, engine.System.SLine),
                    engine.System.SMessageList.HasOwnValue() ? engine.System.SMessageList.GetValue() : engine.List()
                );

                if (!output.Equals(engine.Null))
                {
                    output = ApplyIfDefined(engine, engine.System.SPrePrint, output);

                    if (prompt)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Out[{0}]= {1}", engine.System.SLine.GetValue(), output);
                    }
                    else
                    {
                        Console.WriteLine(output);
                    }
                }

                engine.Evaluate(engine.Expr(engine.System.Increment, engine.System.SLine));
            }
        }

        private static Value ApplyIfDefined(OsmiumEngine engine, Symbol sym, Value value)
        {
            if (engine.System.SPreRead.HasOwnValue())
            {
                value = engine.Evaluate(engine.Expr(sym, value));
            }

            return value;
        }

        private static void AddDownValue(OsmiumEngine engine, Symbol sym, Value value, params Value[] args)
        {
            sym.DownValues.Append((engine.PatternMatching.CompilePattern(engine.Expr(sym, args)), value));
        }
    }
}
