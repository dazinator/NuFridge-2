using System;
using System.Collections;
using System.Collections.Generic;
using NuFridge.Shared.Exceptions;

namespace NuFridge.Shared.Commands.Options
{
    public class OptionValueCollection : IList, ICollection, IList<string>, ICollection<string>, IEnumerable<string>, IEnumerable
    {
        private readonly List<string> _values = new List<string>();
        private readonly OptionContext _c;

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)_values).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)_values).SyncRoot;
            }
        }

        public int Count
        {
            get
            {
                return _values.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                ((IList)_values)[index] = value;
            }
        }

        public string this[int index]
        {
            get
            {
                AssertValid(index);
                if (index < _values.Count)
                    return _values[index];
                return null;
            }
            set
            {
                _values[index] = value;
            }
        }

        internal OptionValueCollection(OptionContext c)
        {
            _c = c;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_values).CopyTo(array, index);
        }

        public void Add(string item)
        {
            _values.Add(item);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(string item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return _values.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        int IList.Add(object value)
        {
            return ((IList)_values).Add(value);
        }

        bool IList.Contains(object value)
        {
            return ((IList)_values).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return ((IList)_values).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            ((IList)_values).Insert(index, value);
        }

        void IList.Remove(object value)
        {
            ((IList)_values).Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            _values.RemoveAt(index);
        }

        public int IndexOf(string item)
        {
            return _values.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            _values.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _values.RemoveAt(index);
        }

        private void AssertValid(int index)
        {
            if (_c.Option == null)
                throw new InvalidOperationException("OptionContext.Option is null.");
            if (index >= _c.Option.MaxValueCount)
                throw new ArgumentOutOfRangeException("index");
            if (_c.Option.OptionValueType == OptionValueType.Required && index >= _values.Count)
                throw new OptionException(string.Format(_c.OptionSet.MessageLocalizer("Missing required value for option '{0}'."), _c.OptionName), _c.OptionName);
        }

        public List<string> ToList()
        {
            return new List<string>(_values);
        }

        public string[] ToArray()
        {
            return _values.ToArray();
        }

        public override string ToString()
        {
            return string.Join(", ", _values.ToArray());
        }
    }
}
