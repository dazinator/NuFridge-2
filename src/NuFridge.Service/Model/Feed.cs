namespace NuFridge.Service.Model
{
    //When adding new properties please remember to add them to the javascript view model so values are not lost on database updates
    public class Feed : IEntityBase
    {

        public string Id { get; set; }
        public string GroupId { get; set; }

        public string Name { get; set; }

        public bool RunPackageCleaner { get; set; }
        public int KeepXNumberOfPackageVersions { get; set; }
    }
}