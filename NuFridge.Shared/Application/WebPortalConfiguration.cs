namespace NuFridge.Shared.Application
{
    public class WebPortalConfiguration : IWebPortalConfiguration
    {

        public string ListenPrefixes{ get; set; }

        public WebPortalConfiguration(IHomeConfiguration config)
        {
            ListenPrefixes = config.ListenPrefixes;
        }
    }
}