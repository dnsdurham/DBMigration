using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleCmdLineApp
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine("Argument Value: " + arg);
            }

            Console.WriteLine("Program complete");
        }
    }
}
