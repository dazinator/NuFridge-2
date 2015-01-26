using System;

namespace NuFridge.DataAccess.Model
{
    public class FeedGroup : IEntityBase
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}