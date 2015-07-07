using System;
using System.Collections.Generic;

namespace NuFridge.Shared.Server.Storage
{
    public class ReferenceCollection : HashSet<string>
    {
        public ReferenceCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ReferenceCollection(string singleValue)
            : this()
        {
            ReplaceAll(new string[1]
            {
                singleValue
            });
        }

        public ReferenceCollection(IEnumerable<string> values)
            : this()
        {
            ReplaceAll(values);
        }

        public void ReplaceAll(IEnumerable<string> newItems)
        {
            Clear();
            if (newItems == null)
                return;
            foreach (string str in newItems)
                Add(str);
        }

        public ReferenceCollection Clone()
        {
            return new ReferenceCollection(this);
        }

        public override string ToString()
        {
            return string.Join(", ", this);
        }

        public static ReferenceCollection One(string item)
        {
            ReferenceCollection referenceCollection = new ReferenceCollection();
            referenceCollection.Add(item);
            return referenceCollection;
        }
    }
}
