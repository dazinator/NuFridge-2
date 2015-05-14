using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using NuFridge.Service.Model;
using NuFridge.Service.Model.Dto;
using NuFridge.Service.Repositories;
using NuGet;
using NuGet.Lucene;
using NuGet.Lucene.Web.Models;
using NuGet.Lucene.Web.Util;

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

            string feedUrl = feed.GetUrl();

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

            string feedUrl = feed.GetUrl();

            var repo = new DataServicePackageRepository(new Uri(feedUrl + "api/odata/"));

            IQueryable<IPackage> query;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                query = repo.GetPackages();
            }
            else
            {
                query = repo.Search(searchTerm, true);
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var results = query
                .Skip(pageSize * page)
                .Take(pageSize)
                .ToList();

            return new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                Results = Mapper.Map<List<DtoPackage>>(results)
            };
        }
    }
}