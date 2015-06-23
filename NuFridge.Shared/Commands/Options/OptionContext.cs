namespace NuFridge.Shared.Commands.Options
{
    public class OptionContext
    {
        private readonly OptionSet _set;
        private readonly OptionValueCollection _c;

        public Option Option { get; set; }

        public string OptionName { get; set; }

        public int OptionIndex { get; set; }

        public OptionSet OptionSet
        {
            get
            {
                return _set;
            }
        }

        public OptionValueCollection OptionValues
        {
            get
            {
                return _c;
            }
        }

        public OptionContext(OptionSet set)
        {
            _set = set;
            _c = new OptionValueCollection(this);
        }
    }
}
