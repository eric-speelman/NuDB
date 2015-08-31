using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NuDB.Exceptions;
using System.Text;
using System.Reflection;

namespace NuDB
{
    public abstract class NuEngine : IDisposable
    {
        public NuEngine() : this("transactions.log") { }
        public NuEngine(string path)
        {
            var types = new Type[] { typeof(NuSet<>), typeof(NuSet<,>) };
            var nuEngineType = typeof(NuEngine);
            var stringType = typeof(string);
            foreach(var set in GetType().GetFields().Where(x => types.Contains(x.FieldType.GetGenericTypeDefinition())))
            {
                if(!set.IsInitOnly)
                {
                    throw new SetNotReadOnlyException($"NuSet field {set.Name} must be readonly");
                }
                if(set.GetValue(this) != null)
                {
                    throw new SetAssignedValueException($"NuEngine field {set.Name} was assigned a value before base constructorer executed");
                }
                var fullPath = Path.GetFullPath(path);
                SetInstance setInstance = null;
                if (!_instanceDictionary.ContainsKey(fullPath))
                {
                    _instanceDictionary.Add(fullPath, new EngineInstance());
                }
                _engineInstance = _instanceDictionary[fullPath];
                setInstance = _instanceDictionary[fullPath].SetInstances.Where(x => x.Name == set.Name).SingleOrDefault();
                if (setInstance == null)
                {
                    setInstance = new SetInstance() { Type = set.FieldType, GenericType = set.FieldType.GetGenericArguments().First(), Name = set.Name, AddWithoutSaving = set.FieldType.GetMethod("AddWithoutSaving"), Instance = set.FieldType.GetConstructor(new Type[2] { nuEngineType, stringType }).Invoke(new object[2] { this, set.Name }) };
                    _engineInstance.SetInstances.Add(setInstance);
                    var fileStream = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    lock(fileStream)
                    {
                        ReadLog(fileStream);
                        _engineInstance.Writer = new StreamWriter(fileStream);
                    }
                }
                else if(setInstance.Type != set.FieldType)
                { 
                    throw new SetTypeMisMatchException($"Field {set.Name} is of type {set.FieldType.Name} but an engine was already created with field type of {setInstance.Type.Name}");
                }
                set.SetValue(this, setInstance.Instance);
            }
        }

        static NuEngine()
        {
            _instanceDictionary = new Dictionary<string, EngineInstance>();
        }
        public void Dispose()
        {
            
        }

        internal void Add<T>(string Name, T item)
        {
            var json = JsonConvert.SerializeObject(item);
            var data = $"ADD {Name} {json}";
            lock(_engineInstance.Writer)
            {
                _engineInstance.Writer.WriteLine(data);
                _engineInstance.Writer.Flush();
            }
            
        }

        private void ReadLog(FileStream stream)
        {
            var reader = new StreamReader(stream);
            var setDictionary = new Dictionary<string, SetInstance>();
            var deserializers = new Dictionary<Type, MethodInfo>();
            while(!reader.EndOfStream)
            {
                var tokens = reader.ReadLine().Split(new char[] { ' ' }, 3);
                if(tokens.Count() != 3)
                {
                    throw new InvalidLogFileException($"Unexpected number of tokens {tokens.Count()}");
                }
                var op = tokens[0].ToLower();
                if(op == "add")
                {
                    var setName = tokens[1];
                    if(!setDictionary.ContainsKey(tokens[1]))
                    {
                        var setInstance = _engineInstance.SetInstances.Where(x => x.Name == setName).SingleOrDefault();
                        setDictionary.Add(setName, setInstance);
                    }
                    var set = setDictionary[setName];
                    if (set != null)
                    {
                        if(!deserializers.ContainsKey(set.GenericType))
                        {
                            deserializers.Add(set.GenericType, typeof(JsonConvert).GetMethods().Where(x => x.Name == "DeserializeObject" && x.IsGenericMethod && x.IsStatic && x.GetParameters().Count() == 1).Single().MakeGenericMethod(new Type[] { set.GenericType }));
                        }
                        var item = deserializers[set.GenericType].Invoke(null, new object[] { tokens[2] });
                        set.AddWithoutSaving.Invoke(set.Instance, new object[] { item });
                    }
                }else
                {
                    throw new InvalidLogFileException($"Unexpected operation {op}");
                }

            }
        }

        private EngineInstance _engineInstance;
        private static Dictionary<string, EngineInstance> _instanceDictionary;
    }
    class EngineInstance
    {
        public EngineInstance()
        {
            SetInstances = new List<SetInstance>();
        }
        public StreamWriter Writer { get; set; }
        public List<SetInstance> SetInstances { get; set; }
    }
    class SetInstance
    {
        public Type Type { get; set; }
        public Type GenericType { get; set; }
        public MethodInfo AddWithoutSaving { get; set; }
        public string Name { get; set; }
        public object Instance { get; set; }
    }
}
