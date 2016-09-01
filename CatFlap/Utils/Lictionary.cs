using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap.Utils
{
    public class Lictionary<Tkey, Tvalue>
    {
        private Dictionary<Tkey, List<Tvalue>> _backing = new Dictionary<Tkey, List<Tvalue>>();

        public void Add(Tkey key, Tvalue value)
        {
            if (!_backing.ContainsKey(key))
            {
                _backing.Add(key, new List<Tvalue>());
                
            }

            _backing[key].Add(value);
        }

        public void Add(Tkey key, List<Tvalue> value)
        {
            _backing.Add(key, value);
        }

        public bool ContainsKey(Tkey key)
        {
            return _backing.ContainsKey(key);
        }

        public ICollection<Tkey> Keys
        {
            get { return _backing.Keys; }
        }

        public bool Remove(Tkey key)
        {
            return _backing.Remove(key);
        }

        public bool TryGetValue(Tkey key, out Tvalue value)
        {
            throw new NotImplementedException("Use TryGetValues");
        }

        public bool TryGetValues(Tkey key, out List<Tvalue> value)
        {
            return _backing.TryGetValue(key, out value);
        }

        public ICollection<Tvalue> Values
        {
            get
            {
                var result = new List<Tvalue>();
                _backing.Values.ToList().ForEach(x =>
                    {
                        result.AddRange(x);   
                    });
                return result;
            }
        }

        public List<Tvalue> this[Tkey key]
        {
            get
            {
                return _backing[key];
            }
            set
            {
                _backing[key] = value;
            }
        }

        public Tvalue this[Tkey key,int index]
        {
            get
            {
                if (!_backing.ContainsKey(key))
                {
                    throw new IndexOutOfRangeException("Key does not exist");
                }

                if (_backing[key].Count < index + 1)
                {
                    throw new IndexOutOfRangeException("Index for the collection specified by the key is out of range");
                }

                return _backing[key][index];
            }
            set
            {
                if (!_backing.ContainsKey(key))
                {
                    throw new IndexOutOfRangeException("Key does not exist");
                }

                if (_backing[key].Count < index + 1)
                {
                    throw new IndexOutOfRangeException("Index for the collection specified by the key is out of range");
                }

                _backing[key][index] = value;
            }
        }

        public void Add(KeyValuePair<Tkey, Tvalue> item)
        {
            if (!_backing.ContainsKey(item.Key))
            {
                _backing.Add(item.Key, new List<Tvalue>());

            }

            _backing[item.Key].Add(item.Value);
        }

        public void Clear()
        {
            _backing.Clear();
        }

        public void Clear(Tkey key)
        {
            _backing[key].Clear();
        }

        public bool Contains(KeyValuePair<Tkey, Tvalue> item)
        {
            if (!_backing.ContainsKey(item.Key))
            {
                return false;
            }

            return _backing[item.Key].Contains(item.Value);
        }

        public void CopyTo(KeyValuePair<Tkey, Tvalue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _backing.Values.Sum(x => x.Count); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<Tkey, Tvalue> item)
        {
            if (!_backing.ContainsKey(item.Key))
            {
                return false;
            }

            return _backing[item.Key].Remove(item.Value);
        }
    }
}
