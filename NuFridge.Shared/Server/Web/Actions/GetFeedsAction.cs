﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Authentication.Token;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions
{
    public class GetFeedsAction : IAction
    {
        private readonly IStore _store;

        public GetFeedsAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(INancyModule module)
        {
            module.RequiresAuthentication();

            int page = int.Parse(module.Request.Query["page"]);
            int pageSize = int.Parse(module.Request.Query["pageSize"]);
            int totalResults;

            List<IFeed> feeds;
            using (ITransaction transaction = _store.BeginTransaction())
            {
                feeds = transaction.Query<IFeed>().ToList(pageSize * page, pageSize, out totalResults);
            }

            var totalPages = (int)Math.Ceiling((double)totalResults / pageSize);

            return new
            {
                TotalCount = totalResults,
                TotalPages = totalPages,
                Results = feeds
            };
        }
    }
}