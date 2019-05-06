using System;

namespace Osmium.Engine.Kernel
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Osmium Kernel");

            var engine = new OsmiumEngine();

            while (true)
            {
                var line = engine.Line;
                Console.WriteLine();
                Console.Write("In[{0}]:= ", line);
                var input = Console.ReadLine();
                var output = engine.Evaluate(input);
                if (!output.Equals(engine.Null))
                {
                    Console.WriteLine();
                    Console.WriteLine("Out[{0}]= {1}", line, output);
                }
            }
        }
    }
}
