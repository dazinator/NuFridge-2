using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace NuFridge.Shared.Commands.Options
{
    public abstract class Option
    {
        private static readonly char[] NameTerminator = new char[2]
    {
      '=',
      ':'
    };
        private readonly string _prototype;
        private readonly string _description;
        private readonly string[] _names;
        private readonly OptionValueType _type;
        private readonly int _count;
        private string[] _separators;

        public string Prototype
        {
            get
            {
                return _prototype;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }

        public OptionValueType OptionValueType
        {
            get
            {
                return _type;
            }
        }

        public int MaxValueCount
        {
            get
            {
                return _count;
            }
        }

        internal string[] Names
        {
            get
            {
                return _names;
            }
        }

        internal string[] ValueSeparators
        {
            get
            {
                return _separators;
            }
        }

        protected Option(string prototype, string description)
            : this(prototype, description, 1)
        {
        }

        protected Option(string prototype, string description, int maxValueCount)
        {
            if (prototype == null)
                throw new ArgumentNullException("prototype");
            if (prototype.Length == 0)
                throw new ArgumentException("Cannot be the empty string.", "prototype");
            if (maxValueCount < 0)
                throw new ArgumentOutOfRangeException("maxValueCount");
            _prototype = prototype;
            _names = prototype.Split('|');
            _description = description;
            _count = maxValueCount;
            _type = ParsePrototype();
            if (_count == 0 && _type != OptionValueType.None)
                throw new ArgumentException("Cannot provide maxValueCount of 0 for OptionValueType.Required or OptionValueType.Optional.", "maxValueCount");
            if (_type == OptionValueType.None && maxValueCount > 1)
                throw new ArgumentException(string.Format("Cannot provide maxValueCount of {0} for OptionValueType.None.", maxValueCount), "maxValueCount");
            if (Array.IndexOf(_names, "<>") >= 0 && (_names.Length == 1 && _type != OptionValueType.None || _names.Length > 1 && MaxValueCount > 1))
                throw new ArgumentException("The default option handler '<>' cannot require values.", "prototype");
        }

        public string[] GetNames()
        {
            return (string[])_names.Clone();
        }

        public string[] GetValueSeparators()
        {
            if (_separators == null)
                return new string[0];
            return (string[])_separators.Clone();
        }

        protected static T Parse<T>(string value, OptionContext c)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            T obj = default(T);
            try
            {
                if (value != null)
                    obj = (T)converter.ConvertFromString(value);
            }
            catch (Exception ex)
            {
                throw new OptionException(string.Format(c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."), value, typeof(T).Name, c.OptionName), c.OptionName, ex);
            }
            return obj;
        }

        private OptionValueType ParsePrototype()
        {
            char ch = char.MinValue;
            List<string> list = new List<string>();
            for (int index1 = 0; index1 < _names.Length; ++index1)
            {
                string name = _names[index1];
                if (name.Length == 0)
                    throw new ArgumentException("Empty option names are not supported.", "prototype");
                int index2 = name.IndexOfAny(NameTerminator);
                if (index2 != -1)
                {
                    _names[index1] = name.Substring(0, index2);
                    if (ch != 0 && ch != name[index2])
                        throw new ArgumentException(string.Format("Conflicting option types: '{0}' vs. '{1}'.", ch, name[index2]), "prototype");
                    ch = name[index2];
                    AddSeparators(name, index2, list);
                }
            }
            if (ch == 0)
                return OptionValueType.None;
            if (_count <= 1 && list.Count != 0)
                throw new ArgumentException(string.Format("Cannot provide key/value separators for Options taking {0} value(s).", _count), "prototype");
            if (_count > 1)
            {
                if (list.Count == 0)
                    _separators = new string[2]
          {
            ":",
            "="
          };
                else
                    _separators = list.Count != 1 || list[0].Length != 0 ? list.ToArray() : null;
            }
            return (int)ch != 61 ? OptionValueType.Optional : OptionValueType.Required;
        }

        private static void AddSeparators(string name, int end, ICollection<string> seps)
        {
            int startIndex = -1;
            for (int index = end + 1; index < name.Length; ++index)
            {
                switch (name[index])
                {
                    case '{':
                        if (startIndex != -1)
                            throw new ArgumentException(string.Format("Ill-formed name/value separator found in \"{0}\".", name), "prototype");
                        startIndex = index + 1;
                        break;
                    case '}':
                        if (startIndex == -1)
                            throw new ArgumentException(string.Format("Ill-formed name/value separator found in \"{0}\".", name), "prototype");
                        seps.Add(name.Substring(startIndex, index - startIndex));
                        startIndex = -1;
                        break;
                    default:
                        if (startIndex == -1)
                        {
                            seps.Add(name[index].ToString(CultureInfo.InvariantCulture));
                        }
                        break;
                }
            }
            if (startIndex != -1)
                throw new ArgumentException(string.Format("Ill-formed name/value separator found in \"{0}\".", name), "prototype");
        }

        public void Invoke(OptionContext c)
        {
            OnParseComplete(c);
            c.OptionName = null;
            c.Option = null;
            c.OptionValues.Clear();
        }

        protected abstract void OnParseComplete(OptionContext c);

        public override string ToString()
        {
            return Prototype;
        }
    }
}
