using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NuFridge.Shared.Commands.Options
{
    public class OptionSet : KeyedCollection<string, Option>
    {
        private readonly Regex _valueOption = new Regex("^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");
        private Action<string[]> _leftovers;
        private readonly Converter<string, string> _localizer;

        public Converter<string, string> MessageLocalizer
        {
            get
            {
                return _localizer;
            }
        }

        public OptionSet()
            : this(f => f)
        {
        }

        public OptionSet(Converter<string, string> localizer)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            _localizer = localizer;
        }

        protected override string GetKeyForItem(Option item)
        {
            if (item == null)
                throw new ArgumentNullException("option");
            if (item.Names != null && item.Names.Length > 0)
                return item.Names[0];
            throw new InvalidOperationException("Option has no names!");
        }

        protected Option GetOptionForName(string option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (!Contains(option))
                return null;
            return this[option];
        }

        protected override void InsertItem(int index, Option item)
        {
            base.InsertItem(index, item);
            AddImpl(item);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            Option option = Items[index];
            for (int index1 = 1; index1 < option.Names.Length; ++index1)
                Dictionary.Remove(option.Names[index1]);
        }

        protected override void SetItem(int index, Option item)
        {
            base.SetItem(index, item);
            RemoveItem(index);
            AddImpl(item);
        }

        private void AddImpl(Option option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            List<string> list = new List<string>(option.Names.Length);
            try
            {
                for (int index = 1; index < option.Names.Length; ++index)
                {
                    Dictionary.Add(option.Names[index], option);
                    list.Add(option.Names[index]);
                }
            }
            catch (Exception ex)
            {
                foreach (string key in list)
                    Dictionary.Remove(key);
                throw;
            }
        }

        public OptionSet Add(Option option)
        {
            base.Add(option);
            return this;
        }

        public OptionSet Add(string prototype, Action<string> action)
        {
            return Add(prototype, null, action);
        }

        public OptionSet Add(string prototype, string description, Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            Add(new ActionOption(prototype, description, 1, v => action(v[0])));
            return this;
        }

        public OptionSet Add(string prototype, OptionAction<string, string> action)
        {
            return Add(prototype, null, action);
        }

        public OptionSet Add(string prototype, string description, OptionAction<string, string> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            Add(new ActionOption(prototype, description, 2, v => action(v[0], v[1])));
            return this;
        }

        public OptionSet Add<T>(string prototype, Action<T> action)
        {
            return Add(prototype, null, action);
        }

        public OptionSet Add<T>(string prototype, string description, Action<T> action)
        {
            return Add(new ActionOption<T>(prototype, description, action));
        }

        public OptionSet Add<TKey, TValue>(string prototype, OptionAction<TKey, TValue> action)
        {
            return Add(prototype, null, action);
        }

        public OptionSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
        {
            return Add(new ActionOption<TKey, TValue>(prototype, description, action));
        }

        protected virtual OptionContext CreateOptionContext()
        {
            return new OptionContext(this);
        }

        public OptionSet WithExtras(Action<string[]> leftovers)
        {
            _leftovers = leftovers;
            return this;
        }

        public List<string> Parse(IEnumerable<string> arguments)
        {
            bool process = true;
            OptionContext c = CreateOptionContext();
            c.OptionIndex = -1;
            Option def = GetOptionForName("<>");
            List<string> list = arguments.Where(argument =>
            {
                if (++c.OptionIndex < 0 || !process && def == null)
                    return true;
                if (!process)
                {
                    if (def == null)
                        return true;
                    return Unprocessed(null, def, c, argument);
                }
                if (argument == "--")
                    return process = false;
                if (Parse(argument, c))
                    return false;
                if (def == null)
                    return true;
                return Unprocessed(null, def, c, argument);
            }).ToList();
            if (c.Option != null)
                c.Option.Invoke(c);
            if (_leftovers != null && list.Count > 0)
                _leftovers(list.ToArray());
            return list;
        }

        private static bool Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
        {
            if (def == null)
            {
                extra.Add(argument);
                return false;
            }
            c.OptionValues.Add(argument);
            c.Option = def;
            c.Option.Invoke(c);
            return false;
        }

        protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
        {
            if (argument == null)
                throw new ArgumentNullException("argument");
            flag = name = sep = value = null;
            Match match = _valueOption.Match(argument);
            if (!match.Success)
                return false;
            flag = match.Groups["flag"].Value;
            name = match.Groups["name"].Value;
            if (match.Groups["sep"].Success && match.Groups["value"].Success)
            {
                sep = match.Groups["sep"].Value;
                value = match.Groups["value"].Value;
            }
            return true;
        }

        protected virtual bool Parse(string argument, OptionContext c)
        {
            if (c.Option != null)
            {
                ParseValue(argument, c);
                return true;
            }
            string flag;
            string name;
            string sep;
            string option1;
            if (!GetOptionParts(argument, out flag, out name, out sep, out option1))
                return false;
            if (Contains(name))
            {
                Option option2 = this[name];
                c.OptionName = flag + name;
                c.Option = option2;
                switch (option2.OptionValueType)
                {
                    case OptionValueType.None:
                        c.OptionValues.Add(name);
                        c.Option.Invoke(c);
                        break;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        ParseValue(option1, c);
                        break;
                }
                return true;
            }
            if (ParseBool(argument, name, c))
                return true;
            return ParseBundledValue(flag, string.Concat(name + sep + option1), c);
        }

        private void ParseValue(string option, OptionContext c)
        {
            if (option != null)
            {
                string[] strArray;
                if (c.Option.ValueSeparators == null)
                    strArray = new string[1]
          {
            option
          };
                else
                    strArray = option.Split(c.Option.ValueSeparators, StringSplitOptions.None);
                foreach (string str in strArray)
                    c.OptionValues.Add(str);
            }
            if (c.OptionValues.Count == c.Option.MaxValueCount || c.Option.OptionValueType == OptionValueType.Optional)
                c.Option.Invoke(c);
            else if (c.OptionValues.Count > c.Option.MaxValueCount)
                throw new OptionException(_localizer(string.Format("Error: Found {0} option values when expecting {1}.", c.OptionValues.Count, c.Option.MaxValueCount)), c.OptionName);
        }

        private bool ParseBool(string option, string n, OptionContext c)
        {
            string index;
            if (n.Length < 1 || n[n.Length - 1] != 43 && n[n.Length - 1] != 45 || !Contains(index = n.Substring(0, n.Length - 1)))
                return false;
            Option option1 = this[index];
            string str = (int)n[n.Length - 1] == 43 ? option : null;
            c.OptionName = option;
            c.Option = option1;
            c.OptionValues.Add(str);
            option1.Invoke(c);
            return true;
        }

        private bool ParseBundledValue(string f, string n, OptionContext c)
        {
            if (f != "-")
                return false;
            for (int index = 0; index < n.Length; ++index)
            {
                string str1 = f + n[index];
                string key = n[index].ToString();
                if (!Contains(key))
                {
                    if (index == 0)
                        return false;
                    throw new OptionException(string.Format(_localizer("Cannot bundle unregistered option '{0}'."), str1), str1);
                }
                Option option = this[key];
                switch (option.OptionValueType)
                {
                    case OptionValueType.None:
                        Invoke(c, str1, n, option);
                        continue;
                    case OptionValueType.Optional:
                    case OptionValueType.Required:
                        string str2 = n.Substring(index + 1);
                        c.Option = option;
                        c.OptionName = str1;
                        ParseValue(str2.Length != 0 ? str2 : null, c);
                        return true;
                    default:
                        throw new InvalidOperationException("Unknown OptionValueType: " + option.OptionValueType);
                }
            }
            return true;
        }

        private static void Invoke(OptionContext c, string name, string value, Option option)
        {
            c.OptionName = name;
            c.Option = option;
            c.OptionValues.Add(value);
            option.Invoke(c);
        }

        public void WriteOptionDescriptions(TextWriter o)
        {
            foreach (Option p in this)
            {
                int written = 0;
                if (WriteOptionPrototype(o, p, ref written))
                {
                    if (written < 29)
                    {
                        o.Write(new string(' ', 29 - written));
                    }
                    else
                    {
                        o.WriteLine();
                        o.Write(new string(' ', 29));
                    }
                    List<string> lines = GetLines(_localizer(GetDescription(p.Description)));
                    o.WriteLine(lines[0]);
                    string str = new string(' ', 31);
                    for (int index = 1; index < lines.Count; ++index)
                    {
                        o.Write(str);
                        o.WriteLine(lines[index]);
                    }
                }
            }
        }

        private bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
        {
            string[] names = p.Names;
            int nextOptionIndex1 = GetNextOptionIndex(names, 0);
            if (nextOptionIndex1 == names.Length)
                return false;
            if (names[nextOptionIndex1].Length == 1)
            {
                Write(o, ref written, "  -");
                Write(o, ref written, names[0]);
            }
            else
            {
                Write(o, ref written, "      --");
                Write(o, ref written, names[0]);
            }
            for (int nextOptionIndex2 = GetNextOptionIndex(names, nextOptionIndex1 + 1); nextOptionIndex2 < names.Length; nextOptionIndex2 = GetNextOptionIndex(names, nextOptionIndex2 + 1))
            {
                Write(o, ref written, ", ");
                Write(o, ref written, names[nextOptionIndex2].Length == 1 ? "-" : "--");
                Write(o, ref written, names[nextOptionIndex2]);
            }
            if (p.OptionValueType == OptionValueType.Optional || p.OptionValueType == OptionValueType.Required)
            {
                if (p.OptionValueType == OptionValueType.Optional)
                    Write(o, ref written, _localizer("["));
                Write(o, ref written, _localizer("=" + GetArgumentName(0, p.MaxValueCount, p.Description)));
                string str = p.ValueSeparators == null || p.ValueSeparators.Length <= 0 ? " " : p.ValueSeparators[0];
                for (int index = 1; index < p.MaxValueCount; ++index)
                    Write(o, ref written, _localizer(str + GetArgumentName(index, p.MaxValueCount, p.Description)));
                if (p.OptionValueType == OptionValueType.Optional)
                    Write(o, ref written, _localizer("]"));
            }
            return true;
        }

        private static int GetNextOptionIndex(string[] names, int i)
        {
            while (i < names.Length && names[i] == "<>")
                ++i;
            return i;
        }

        private static void Write(TextWriter o, ref int n, string s)
        {
            n += s.Length;
            o.Write(s);
        }

        private static string GetArgumentName(int index, int maxIndex, string description)
        {
            if (description == null)
            {
                if (maxIndex != 1)
                    return "VALUE" + (index + 1);
                return "VALUE";
            }
            string[] strArray;
            if (maxIndex == 1)
                strArray = new string[2]
        {
          "{0:",
          "{"
        };
            else
                strArray = new string[1]
        {
          "{" + index + ":"
        };
            for (int index1 = 0; index1 < strArray.Length; ++index1)
            {
                int startIndex1 = 0;
                int startIndex2;
                do
                {
                    startIndex2 = description.IndexOf(strArray[index1], startIndex1);
                }
                while ((startIndex2 < 0 || startIndex1 == 0 ? 0 : ((int)description[startIndex1++ - 1] == 123 ? 1 : 0)) != 0);
                if (startIndex2 != -1)
                {
                    int num = description.IndexOf("}", startIndex2);
                    if (num != -1)
                        return description.Substring(startIndex2 + strArray[index1].Length, num - startIndex2 - strArray[index1].Length);
                }
            }
            if (maxIndex != 1)
                return "VALUE" + (index + 1);
            return "VALUE";
        }

        private static string GetDescription(string description)
        {
            if (description == null)
                return string.Empty;
            StringBuilder stringBuilder = new StringBuilder(description.Length);
            int startIndex = -1;
            for (int index = 0; index < description.Length; ++index)
            {
                switch (description[index])
                {
                    case ':':
                        if (startIndex >= 0)
                        {
                            startIndex = index + 1;
                            break;
                        }
                        goto default;
                    case '{':
                        if (index == startIndex)
                        {
                            stringBuilder.Append('{');
                            startIndex = -1;
                            break;
                        }
                        if (startIndex < 0)
                        {
                            startIndex = index + 1;
                        }
                        break;
                    case '}':
                        if (startIndex < 0)
                        {
                            if (index + 1 == description.Length || description[index + 1] != 125)
                                throw new InvalidOperationException("Invalid option description: " + description);
                            ++index;
                            stringBuilder.Append("}");
                            break;
                        }
                        stringBuilder.Append(description.Substring(startIndex, index - startIndex));
                        startIndex = -1;
                        break;
                    default:
                        if (startIndex < 0)
                        {
                            stringBuilder.Append(description[index]);
                        }
                        break;
                }
            }
            return stringBuilder.ToString();
        }

        private static List<string> GetLines(string description)
        {
            List<string> list1 = new List<string>();
            if (string.IsNullOrEmpty(description))
            {
                list1.Add(string.Empty);
                return list1;
            }
            int length = 49;
            int index1 = 0;
            int lineEnd;
            do
            {
                lineEnd = GetLineEnd(index1, length, description);
                bool flag = false;
                if (lineEnd < description.Length)
                {
                    char c = description[lineEnd];
                    if (c == 45 || char.IsWhiteSpace(c) && c != 10)
                        ++lineEnd;
                    else if (c != 10)
                    {
                        flag = true;
                        --lineEnd;
                    }
                }
                list1.Add(description.Substring(index1, lineEnd - index1));
                if (flag)
                {
                    List<string> list2;
                    int index2;
                    (list2 = list1)[index2 = list1.Count - 1] = list2[index2] + "-";
                }
                index1 = lineEnd;
                if (index1 < description.Length && description[index1] == 10)
                    ++index1;
            }
            while (lineEnd < description.Length);
            return list1;
        }

        private static int GetLineEnd(int start, int length, string description)
        {
            int num1 = Math.Min(start + length, description.Length);
            int num2 = -1;
            for (int index = start; index < num1; ++index)
            {
                switch (description[index])
                {
                    case ',':
                    case '-':
                    case '.':
                    case ';':
                    case '\t':
                    case '\v':
                    case ' ':
                        num2 = index;
                        break;
                    case '\n':
                        return index;
                }
            }
            if (num2 == -1 || num1 == description.Length)
                return num1;
            return num2;
        }

        private sealed class ActionOption : Option
        {
            private readonly Action<OptionValueCollection> _action;

            public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action)
                : base(prototype, description, count)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                _action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(c.OptionValues);
            }
        }

        private sealed class ActionOption<T> : Option
        {
            private readonly Action<T> _action;

            public ActionOption(string prototype, string description, Action<T> action)
                : base(prototype, description, 1)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                _action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(Parse<T>(c.OptionValues[0], c));
            }
        }

        private sealed class ActionOption<TKey, TValue> : Option
        {
            private readonly OptionAction<TKey, TValue> _action;

            public ActionOption(string prototype, string description, OptionAction<TKey, TValue> action)
                : base(prototype, description, 2)
            {
                if (action == null)
                    throw new ArgumentNullException("action");
                _action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(Parse<TKey>(c.OptionValues[0], c), Parse<TValue>(c.OptionValues[1], c));
            }
        }
    }
}
