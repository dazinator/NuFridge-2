using System;

namespace NuFridge.DataAccess.Model
{
    //When adding new properties please remember to add them to the javascript view model so values are not lost on database updates
    public class Feed : IEntityBase
    {

        public Guid Id { get; set; }
        public Guid GroupId { get; set; }

        public string Name { get; set; }
    }
}