using System;
using System.Collections.Generic;

namespace Green.Repl
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var repl = new Repl(args);
            return repl.Run();
        }
    }
}
