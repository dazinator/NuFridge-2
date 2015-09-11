using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Model
{
    [Table("FeedGroup", Schema = "NuFridge")]
    public class FeedGroup : IFeedGroup
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Editable(false)]
        public IEnumerable<Feed> Feeds { get; set; } 
    }

    public interface IFeedGroup
    {
        int Id { get; }
        string Name { get; }
        IEnumerable<Feed> Feeds { get; }
    }
}