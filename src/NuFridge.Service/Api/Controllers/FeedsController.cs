using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NuFridge.DataAccess.Model;
using NuFridge.DataAccess.Repositories;

namespace NuFridge.Service.Api.Controllers
{
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