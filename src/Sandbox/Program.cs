using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime;
using NuDB;

namespace Sandbox
{
    public class Program
    {
        public void Main(string[] args)
        {
            var engine = new DbEngine();
            var s = new Something();
            s.Int = 5;
            s.String = "Aaaaaaaaa";
            s.List = new List<int>() { 1, 2, 3, 4 };
            engine.Ints.Add(s);
            
            Console.In.ReadLine();
        }
    }

    public class DbEngine : NuEngine
    {
        public readonly NuSet<Something> Ints;
    }

    public class Something
    {
        public int Int { get; set; }
        public string String { get; set; }
        public List<int> List { get; set; }
    }
}
