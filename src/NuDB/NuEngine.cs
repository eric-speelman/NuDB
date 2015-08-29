using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NuDB.Exceptions;
using System.Text;


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
                    setInstance = new SetInstance() { Type = set.FieldType, Name = set.Name, Instance = set.FieldType.GetConstructor(new Type[2] { nuEngineType, stringType }).Invoke(new object[2] { this, set.Name }) };
                    _engineInstance.SetInstances.Add(setInstance);
                    _engineInstance.FileStream = File.Open(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    lock(_engineInstance.FileStream)
                    {
                        var streamReader = new StreamReader(_engineInstance.FileStream).ReadToEnd();
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
            var data = JsonConvert.SerializeObject(item);
            var bytes = Encoding.UTF8.GetBytes($"ADD {Name} {data}\n");
            lock(_engineInstance.FileStream)
            {
                _engineInstance.FileStream.Write(bytes, 0, bytes.Count());
            }
            
        }

        private void ReadLog(SetInstance instance)
        {
        
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
        public FileStream FileStream { get; set; }
        public List<SetInstance> SetInstances { get; set; }
    }
    class SetInstance
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public object Instance { get; set; }
    }
}
