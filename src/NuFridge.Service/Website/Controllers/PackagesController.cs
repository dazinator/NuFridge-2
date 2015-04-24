using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using NuFridge.Service.Feeds;
using NuFridge.Service.Model;
using NuFridge.Service.Model.Dto;
using NuFridge.Service.Repositories;
using NuGet;

namespace NuFridge.Service.Website.Controllers
{
    [Route("api/packages")]
    public class PackagesController : ApiController
    {
        private IRepository<Feed> FeedRepository { get; set; }

        public PackagesController()
        {
            FeedRepository = new SqlCompactRepository<Feed>();
        }

        [HttpGet]
        public object Get(string id, int page, int pageSize, string searchTerm = "")
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


            var baseUrl = new ServiceConfiguration().FeedWebBinding.Replace("*", "localhost");

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            var baseAddress = string.Format("{0}{1}/", baseUrl, feed.Name);

            var repo = new DataServicePackageRepository(new Uri(baseAddress + "api/odata/"));

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
                .Skip(pageSize*page)
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