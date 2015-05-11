using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NuFridge.Service.Authentication.Managers;
using NuFridge.Service.Diagnostics;
using NuFridge.Service.Model;
using NuFridge.Service.Repositories;

namespace NuFridge.Service.Website.Controllers
{
    public class DashboardController : ApiController
    {
        private IRepository<Feed> FeedRepository { get; set; }
        private IdentityManager IdentityRepository { get; set; }

        public DashboardController()
        {
            FeedRepository = new SqlCompactRepository<Feed>();
            IdentityRepository = new IdentityManager();
        }

        [Route("api/dashboard")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var feeds = FeedRepository.GetAll();
            var usersCount = IdentityRepository.GetUsersCount();


            return Request.CreateResponse(new
            {
                FeedCount = feeds.Count(),
                userCount = usersCount
            });
        }
    }
}