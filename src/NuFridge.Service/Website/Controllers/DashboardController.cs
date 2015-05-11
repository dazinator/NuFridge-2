using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NuFridge.Service.Diagnostics;
using NuFridge.Service.Model;
using NuFridge.Service.Repositories;

namespace NuFridge.Service.Website.Controllers
{
    public class DashboardController : ApiController
    {
        private IRepository<Feed> FeedRepository { get; set; }

        public DashboardController()
        {
            FeedRepository = new SqlCompactRepository<Feed>();
        }

        [Route("api/dashboard")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var feeds = FeedRepository.GetAll();

            return Request.CreateResponse(new
            {
                FeedCount = feeds.Count()
            });
        }
    }
}