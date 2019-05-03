using System;
using System.Collections.Generic;

namespace Green.Repl
{
    public class Repl
    {
        private readonly Interpreter _interpreter;
        private readonly Reader _reader;
        private readonly string[] _args;

        public Repl(string[] args)
        {
            _args = args;
            _interpreter = new Interpreter();
            _reader = new Reader();
        }

        public int Run()
        {
            Console.WriteLine("Green REPL");
            var lines = new List<string>();
            while (true)
            {
                if (lines.Count == 0)
                    Console.Write("Green> ");
                else
                    Console.Write(". ");

                lines.Add(Console.ReadLine());
                var result = _reader.ReadInteractive(lines);
                if (result.Finished)
                {
                    Eval(result.Objects);
                    lines.Clear();
                }
            }
        }

        public void Eval(IReadOnlyList<ISyntax> objects)
        {
            var results = new List<object>();

            foreach (var o in objects)
            {
                var result = _interpreter.Eval(o);
                if (result != null)
                    results.Add(result);
            }

            foreach (var result in results)
                Console.WriteLine(result);
        }
    }
}
