using Dapper;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Model
{
    [Table("Statistic", Schema = "NuFridge")]
    public class Statistic : IStatistic
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Json { get; set; }

        public Statistic()
        {
            
        }

        public Statistic(string name)
        {
            Name = name;
        }
    }

    public interface IStatistic
    {
        int Id { get; set; }

        string Name { get; set; }

        string Json { get; set; }
    }
}