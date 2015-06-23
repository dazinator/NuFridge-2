namespace NuFridge.Shared.Server.Storage
{
    public class UniqueRule
    {
        public string Message { get; set; }

        public string ConstraintName { get; set; }

        public string[] Columns { get; set; }

        public UniqueRule(string constraintName, params string[] columns)
        {
            ConstraintName = constraintName;
            Columns = columns;
        }
    }
}
