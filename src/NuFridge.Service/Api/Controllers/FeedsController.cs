using System;
using System.Collections.Generic;
using System.Web.Http;
using NuFridge.Service.Data.Model;
using NuFridge.Service.Data.Repositories;

namespace NuFridge.Service.Api.Controllers
{
    [Authorize]
    public class FeedsController : ApiController 
    {
        private IRepository<Feed> FeedRepository { get; set; }

        public FeedsController()
        {
            FeedRepository = new SqlCompactRepository<Feed>();
        }

        // GET api/feeds 
        public IEnumerable<Feed> Get()
        {
            return FeedRepository.GetAll();
        }

        // GET api/feeds/5 
        public Feed Get(Guid id)
        {
            return FeedRepository.GetById(id);
        }

        // POST api/feeds
        public void Post([FromBody]Feed value)
        {
        }

        // PUT api/feeds/5 
        public void Put(Guid id, [FromBody]Feed value)
        {
        }

        // DELETE api/feeds/5 
        public void Delete(Guid id)
        {
        } 
    }
}