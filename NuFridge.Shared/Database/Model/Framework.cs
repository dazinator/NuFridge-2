using Dapper;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Model
{
    [Table("Framework", Schema = "NuFridge")]
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