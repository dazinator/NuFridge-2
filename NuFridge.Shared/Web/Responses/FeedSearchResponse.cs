using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuFridge.Shared.Web.Responses
{
    //http://beta.semantic-ui.com/modules/search.html#/usage
    public class FeedSearchResponse
    {
        [JsonProperty("results")]
        public List<Category> Results { get; set; }

        public FeedSearchResponse()
        {
            Results = new List<Category>();
        }


        public class Category
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("results")]
            public List<FeedResult> Feeds { get; set; } 

            public Category() : this(null)
            {
                
            }

            public Category(string name)
            {
                Name = name;
                Feeds = new List<FeedResult>();
            }

            public class FeedResult
            {
                [JsonProperty("title")]
                public string Title { get; set; }

                [JsonProperty("url")]
                public string Url { get; set; }

                public FeedResult()
                {
                    
                }

                public FeedResult(string title, string url)
                {
                    Title = title;
                    Url = url;
                }
            }
        }
    }
}