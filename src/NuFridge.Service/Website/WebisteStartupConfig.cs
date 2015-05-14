﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json.Serialization;
using NuFridge.Service.Authentication.Providers;
using NuFridge.Service.Website;
using NuFridge.Service.Website.Filters;
using NuFridge.Service.Website.Middleware;
using Owin;

[assembly: OwinStartup(typeof(WebisteStartupConfig))]

namespace NuFridge.Service.Website
{
    public class WebisteStartupConfig
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            ConfigureOAuth(appBuilder);

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(config, Assembly.Load("NuGet.Lucene.Web")));


            config.MapHttpAttributeRoutes();

            config.Filters.Add(new ValidationActionFilter());

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var currentAssemblyPath = Assembly.GetEntryAssembly().Location;
            var currentDirectory = Directory.GetParent(currentAssemblyPath);

            string appPath;

            if (currentDirectory.FullName.EndsWith(@"\bin\Debug"))
            {
                var root = Directory.GetParent(Directory.GetParent(currentDirectory.FullName).FullName).FullName;

                appPath = Path.Combine(root, "Website", "Content");
            }
            else
            {
                appPath = Path.Combine(currentDirectory.FullName, "Content");
            }

            var physicalFileSystem = new PhysicalFileSystem(appPath);
            var options = new FileServerOptions
            {
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem
            };

            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };
            appBuilder.UseFileServer(options);
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.UseWebApi(config);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions oAuthServerOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider()
            };

            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
            app.Use(typeof(MiddlewareLogger));
        }
    }

}