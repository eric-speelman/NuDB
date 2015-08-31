using Xunit;
using System.Linq;
using System.Collections.Generic;
using NuDB;
using NuDB.Exceptions;

namespace NuDB.Test
{
    public class EngineTests
    {
        [Fact]
        public void ThrowsSetNotReadOnlyException()
        {
            Assert.Throws<SetNotReadOnlyException>(() => new ReadOnlyExceptionEngine());
        }

        [Fact]
        public void ThrowsSetAssignedValueException()
        {
            Assert.Throws<SetAssignedValueException>(() => new SetAssignedExceptionEngine());
        }

        [Fact]
        public void ThrowsSetTypeMisMatchException()
        {
            var engine = new Engine();
            Assert.Throws<SetTypeMisMatchException>(() => new TypeMisMatchEngine());
        }

        [Fact]
        public void EngineInitsSets()
        {
            var engine = new Engine();
            Assert.True(engine.Ints != null);
        }

        [Fact]
        public void MultipleSetsAccessSameMemeory()
        {
            var engine = new Engine();
            engine.Ints.Add(3);
            Assert.Equal(1, engine.Ints.Count());
            Assert.Equal(3, engine.Ints.Single());
            var engine2 = new Engine();
            Assert.Equal(1, engine2.Ints.Count());
            Assert.Equal(3, engine2.Ints.Single());
            engine.Ints.Add(6);
            Assert.Equal(2, engine.Ints.Count());
            Assert.Equal(6, engine.Ints.Last());
            Assert.Equal(2, engine2.Ints.Count());
            Assert.Equal(6, engine2.Ints.Last());
            var engine3 = new Engine2();
            Assert.Equal(2, engine3.Ints.Count());
            Assert.Equal(6, engine3.Ints.Last());
        }

        [Fact]
        public void SavesComplexObjects()
        {
            var engine = new ComplexEngine();
            for (var i = 0; i < 1000000; i++)
            {
                var obj = new ComplexObj();
                obj.SomeInt = 5;
                obj.SomeString = "aaaa\n\n\n\nbbbbbbbb";
                obj.StringList = new List<string>() { "aaaaaa", "bbbbbbbbbbb", "cccccc\n\n\n\nn\"ddddddd", "eee   eeee  eeeeee" };
                engine.Obj.Add(obj);
            }
        }
    }

    class ReadOnlyExceptionEngine : NuEngine
    {
        public NuSet<int> Ints = null;
    }

    class SetAssignedExceptionEngine : NuEngine
    {
        public readonly NuSet<int> Ints = new NuSet<int>(new Engine(), "Ints");
    }

    class TypeMisMatchEngine : NuEngine
    {
        public readonly NuSet<long> Ints = null;
    }
    class Engine : NuEngine
    {
        public readonly NuSet<int> Ints = null;
    }

    class Engine2 : NuEngine
    {
        public readonly NuSet<int> Ints = null;
    }

    class ComplexEngine : NuEngine
    {
        public readonly NuSet<ComplexObj> Obj = null;
    }

    class ComplexObj
    {
        public int SomeInt { get; set; }
        public string SomeString { get; set; }
        public IEnumerable<string> StringList { get; set; }
        public IEnumerable<ComplexObj> ComplexList {get;set;}
    }
}
