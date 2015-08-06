namespace NuFridge.Shared.Database.Model
{
    [Dapper.Table("Framework", Schema = "NuFridge")]
    public class Framework : IFramework
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public interface IFramework
    {
        int Id { get; set; }
        string Name { get; set; }
    }
}