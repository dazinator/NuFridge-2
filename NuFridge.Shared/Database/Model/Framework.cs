namespace NuFridge.Shared.Database.Model
{
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