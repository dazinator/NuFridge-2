using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
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

        [EnableQuery]
        public HttpResponseMessage Get(string id = "")
        {
            if (id == string.Empty)
            {
                return Request.CreateResponse(FeedRepository.GetAll());
            }

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

            return Request.CreateResponse(feed);
        }

        public HttpResponseMessage Post([FromBody]Feed feed)
        {
            FeedRepository.Insert(feed);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage Put(string id, [FromBody]Feed feed)
        {
            if (id != feed.Id)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Feed id does not match.");
            }

            FeedRepository.Update(feed);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage Delete(string id)
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

            FeedRepository.Delete(feed);

            return Request.CreateResponse(HttpStatusCode.OK);
        } 
    }
}