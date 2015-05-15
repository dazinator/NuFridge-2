using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using NuFridge.Service.Model;
using NuFridge.Service.Model.Dto;
using NuFridge.Service.Repositories;
using NuFridge.Service.Website.Controllers.Responses;
using NuGet;
using NuGet.Lucene;

namespace NuFridge.Service.Website.Controllers
{
    public class PackagesController : ApiController
    {
        private IRepository<Feed> FeedRepository { get; set; }

        public PackagesController()
        {
            FeedRepository = new SqlCompactRepository<Feed>();
        }

        [AcceptVerbs("POST")]
        [Route("api/packages/{feedId}")]
        public async Task<HttpResponseMessage> UploadFile(string feedId)
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var feed = FeedRepository.GetById(feedId);

            if (feed == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("No feed with ID = {0}", feedId)),
                    ReasonPhrase = "Feed not found"
                };
                throw new HttpResponseException(resp);
            }

            string feedUrl = feed.GetBaseUrl();

            var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());

            //access files
            IList<HttpContent> files = provider.Files;

            //Example: reading a file's stream like below
            HttpContent file1 = files[0];

            Stream stream = await file1.ReadAsStreamAsync();
            var size = stream.Length;

            var apiUrl = new Uri(feedUrl + "api/packages/");

            PackageServer packageServer = new PackageServer(apiUrl.ToString(), "NuFridge");

            var package = new LucenePackage(s => stream);

            try
            {
                packageServer.PushPackage(feed.ApiKey ?? "", package, size, 1000000, false);
            }
            catch (Exception ex)
            {
                var baseEx = ex.GetBaseException();
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(baseEx.Message),
                    ReasonPhrase = baseEx.Message
                };
                throw new HttpResponseException(resp);
            }


            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("api/packages/{id}/{page}/{pageSize}/{searchTerm?}")]
        public Object Get(string id, int page, int pageSize, string searchTerm = "")
        {
            var feed = FeedRepository.GetById(id);

            if (feed == null)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("No feed with ID = {0}", id)),
                    ReasonPhrase = "Feed not found"
                };
                throw new HttpResponseException(resp);
            }

            string packagesUrl = feed.GetPackagesUrl();

  
            WebClient webClient = new WebClient();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                webClient.QueryString.Add("query", searchTerm);
            }
            webClient.QueryString.Add("offset", (pageSize * page).ToString());
            webClient.QueryString.Add("count", pageSize.ToString());
            webClient.QueryString.Add("originFilter", "Any");
            webClient.QueryString.Add("sort", "Score");
            webClient.QueryString.Add("order", "Ascending");
            webClient.QueryString.Add("includePrerelease", "true");
            string json = webClient.DownloadString(packagesUrl);


            var result = JsonConvert.DeserializeObject<PackageQueryResponse>(json);

            return new
            {
                TotalCount = result.totalHits,
                TotalPages = (result.totalHits + pageSize - 1) / pageSize,
                Results = result.hits
            };
        }
    }
}