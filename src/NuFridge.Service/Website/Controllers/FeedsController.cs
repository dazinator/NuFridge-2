using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.Routing;
using NuFridge.Service.Feeds;
using NuFridge.Service.Model;
using NuFridge.Service.Repositories;

namespace NuFridge.Service.Website.Controllers
{
    //[Authorize]
    public class FeedsController : ApiController 
    {
        private IRepository<Feed> FeedRepository { get; set; }

        public FeedsController()
        {
            FeedRepository = new SqlCompactRepository<Feed>();
        }

        public Object Get(int page = 0, int pageSize = 5)
        {
            IList<Feed> query = FeedRepository.GetAll();

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var urlHelper = new UrlHelper(Request);

            var results = query
                .Skip(pageSize*page)
                .Take(pageSize)
                .ToList();

            return new
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                Results = results
            };
        }

        public HttpResponseMessage Get(string id)
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

            return Request.CreateResponse(feed);
        }

        //[EnableQuery]
        //public HttpResponseMessage Get(string id = "")
        //{
        //    if (id == string.Empty)
        //    {
        //        return Request.CreateResponse(FeedRepository.GetAll());
        //    }

        //    var feed = FeedRepository.GetById(id);

        //    if (feed == null)
        //    {
        //        var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
        //        {
        //            Content = new StringContent(string.Format("No feed with ID = {0}", id)),
        //            ReasonPhrase = "Feed not found"
        //        };
        //        throw new HttpResponseException(resp);
        //    }

        //    return Request.CreateResponse(feed);
        //}

        public HttpResponseMessage Post([FromBody]Feed feed)
        {
            FeedRepository.Insert(feed);

            FeedManager.Instance().Start(feed);

            return Request.CreateResponse(HttpStatusCode.OK, feed);
        }

        public HttpResponseMessage Put(string id, [FromBody]Feed feed)
        {
            if (id != feed.Id)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Feed id does not match.");
            }

            FeedManager.Instance().Stop(feed);

            FeedRepository.Update(feed);

            FeedManager.Instance().Start(feed);

            return Request.CreateResponse(HttpStatusCode.OK, feed);
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

            var success = FeedManager.Instance().Stop(feed);

            if (!success)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(string.Format("Failed to stop feed with ID = {0}", id)),
                    ReasonPhrase = "Failed to stop feed."
                };
                throw new HttpResponseException(resp);
            }

            FeedRepository.Delete(feed);

            return Request.CreateResponse(HttpStatusCode.OK, feed);
        } 
    }
}