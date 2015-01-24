﻿using MongoDB.Driver;
using NuFridge.Common.Helpers;
using NuFridge.Common.Manager;
using NuFridge.Common.Managers.IIS;
using NuFridge.DataAccess.Connection;
using NuFridge.DataAccess.Repositories;
using NuFridge.Website.MVC.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NuFridge.Website.MVC.Controllers.API
{
    public class InstallationController : ApiController
    {
        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Mvc.HttpGet]
        public HttpResponseMessage IsInstallationValid()
        {
            try
            {
                string message;
                string feedWebsiteName;

                if (!ConfigHelper.GetFeedWebsiteName(out message, out feedWebsiteName))
                    throw new Exception(message);

                var databaseSystemName = ConfigHelper.GetDatabaseSystemName();
                if (string.IsNullOrWhiteSpace(databaseSystemName))
                {
                    throw new Exception("You must complete install wizard and select a database");
                }

                bool canConnectToBackend = false;
                if (databaseSystemName == "Mongo")
                {
                    var mongoRead = new MongoRead(false);
                    canConnectToBackend = mongoRead.Connect();
                }
                if (databaseSystemName == "Sql Compact")
                {
                    using (var context = new NuFridgeContext())
                    {
                        canConnectToBackend = context.Database.Exists();
                    }
                }

                var hasValidFeedWebsite = new WebsiteManager(feedWebsiteName).WebsiteExists();

                if (canConnectToBackend && hasValidFeedWebsite)
                {
                    return Request.CreateResponse<bool>(HttpStatusCode.OK, true);
                }
            }
            catch (Exception ex)
            {

            }

            return Request.CreateResponse<bool>(HttpStatusCode.OK, false);
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Mvc.HttpGet]
        public HttpResponseMessage GetInstallation()
        {
            try
            {
                NuFridgeInstall install = new NuFridgeInstall();

                string message;
                string feedWebsiteName = string.Empty;
                ConfigHelper.GetFeedWebsiteName(out message, out feedWebsiteName);

                install.DatabaseSystem = ConfigHelper.GetDatabaseSystemName();

                install.IISWebsiteName = feedWebsiteName;
                install.PortNumber = ConfigHelper.GetFeedWebsitePortNumber();
                install.PhysicalDirectory = ConfigHelper.GetFeedWebsitePhysicalPath();
                install.MongoDBServer = ConfigHelper.GetMongoDBServerName();
                install.MongoDBDatabase = ConfigHelper.GetMongoDBDatabaseName();

                return Request.CreateResponse<NuFridgeInstall>(HttpStatusCode.OK, install);
            }
            catch (Exception ex)
            {

            }

            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [System.Web.Mvc.HttpPost]
        public HttpResponseMessage PostInstallation(NuFridgeInstall install)
        {
            try
            {
                WebsiteManager websiteManager = new WebsiteManager(install.IISWebsiteName);
                if (!websiteManager.WebsiteExists())
                {
                    var websiteInfo = new CreateWebsiteArgs(install.PhysicalDirectory)
                        {
                            HostName = "*",
                            PortNumber = install.PortNumber
                        };
                    websiteManager.CreateWebsite(websiteInfo);
                }

                var configFile = ConfigHelper.OpenConfigFile(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, install.IISWebsiteName);

                configFile.AppSettings.Settings[ConfigHelper.FeedWebsitePortNumberKey].Value = install.PortNumber.ToString();
                configFile.AppSettings.Settings[ConfigHelper.FeedWebsitePathKey].Value = install.PhysicalDirectory;

                configFile.AppSettings.Settings[ConfigHelper.FeedWebsiteNameKey].Value = install.IISWebsiteName;

                configFile.AppSettings.Settings[ConfigHelper.DatabaseSystemNameKey].Value = install.DatabaseSystem;
                configFile.AppSettings.Settings[ConfigHelper.MongoDBDatabaseNameKey].Value = install.MongoDBDatabase;
                configFile.AppSettings.Settings[ConfigHelper.MongoDBServerNameKey].Value = install.MongoDBServer;
                configFile.AppSettings.Settings[ConfigHelper.MongoDBConnectionStringKey].Value = string.Format("mongodb://{0}", install.MongoDBServer);

                if (install.DatabaseSystem == "Mongo")
                {
                    if (
                   !MongoRead.TestConnectionString(
                       configFile.AppSettings.Settings[ConfigHelper.MongoDBConnectionStringKey].Value))
                    {
                        return Request.CreateResponse(HttpStatusCode.InternalServerError,
                                                      new HttpError("Could not connect to the MongoDB server."));
                    }

                    if (
                        !MongoRead.TestDatabaseExists(
                            configFile.AppSettings.Settings[ConfigHelper.MongoDBConnectionStringKey].Value,
                            configFile.AppSettings.Settings[ConfigHelper.MongoDBDatabaseNameKey].Value))
                    {
                        MongoRead.CreateDatabase(
                            configFile.AppSettings.Settings[ConfigHelper.MongoDBConnectionStringKey].Value,
                            configFile.AppSettings.Settings[ConfigHelper.MongoDBDatabaseNameKey].Value);
                    }
                }

                if (install.DatabaseSystem == "Sql Compact")
                {
                    using (var context = new NuFridgeContext())
                    {
                        context.Database.Initialize(true);
                    }
                }


                configFile.Save(ConfigurationSaveMode.Minimal);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }

            return Request.CreateResponse<NuFridgeInstall>(HttpStatusCode.OK, install);
        }
    }
}