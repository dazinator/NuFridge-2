using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Service.Plugin.Settings
{
    public class GridSetting : SettingBase
    {
        public List<GridColumn> Columns { get; private set; }
        public GridColumnRowCollection Rows { get; private set; }

        public GridSetting(string identifier, string displayName)
            : base(identifier, displayName)
        {
            Columns = new List<GridColumn>();
            Rows = new GridColumnRowCollection();
        }

        public GridSetting WithColumns(IEnumerable<GridColumn> gridColumn)
        {
            Columns = gridColumn.ToList();

            return this;
        }

        public class GridColumn
        {
            public string Identifier { get; private set; }
            public string DisplayName { get; private set; }

            public GridColumn(string identifier, string displayName)
            {
                Identifier = identifier;
                DisplayName = displayName;
            }
        }

        public class GridColumnRowCollection
        {
            private List<GridColumnRow> Rows { get; set; }

            public GridColumnRowCollection()
            {
                Rows = new List<GridColumnRow>();
            }

            public object this[int number]
            {
                get { return Rows[number]; }
            }

            public IEnumerator<GridColumnRow> GetEnumerator()
            {
                return Rows.GetEnumerator();
            }
        }

        public class GridColumnRow
        {
            private Dictionary<string, object> _values { get; set; }

            public GridColumnRow()
            {
                _values = new Dictionary<string, object>();
            }

            public object this[string column]
            {
                get { return _values[column]; }
            }

            public T GetValue<T>(string column)
            {
                var value = this[column];
                if (value == null)
                {
                    return default(T);
                }

                return (T) value;
            }
        }
    }
}