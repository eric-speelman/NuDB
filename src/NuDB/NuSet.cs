using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;
using NuDB.Exceptions;
using System.IO;

namespace NuDB
{
    public class NuSet<TData, TCollection> : ICollection<TData> where TCollection : ICollection<TData>, new()
    {
        public NuSet(NuEngine nuEngine, string name)
        {
            _collection = new TCollection();
            _nuEngine = nuEngine;
            _name = name;
        }

        public int Count
        {
            get
            {
                return _collection.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _collection.IsReadOnly;
            }
        }

        public void Add(TData item)
        {
            _nuEngine.Add(_name, item);
            _collection.Add(item);
        }

        public void AddWithoutSaving(TData item)
        {
            _collection.Add(item);
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public bool Contains(TData item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(TData[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TData> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public bool Remove(TData item)
        {
            return _collection.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_collection).GetEnumerator();
        }

        private string _name;
        private ICollection<TData> _collection;
        private NuEngine _nuEngine;
    }

    public class NuSet<T> : NuSet<T, List<T>>
    {
        public NuSet(NuEngine engine, string name) : base(engine, name) { }
    }
}
