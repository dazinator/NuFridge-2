using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac;
using Nancy;
using Nancy.Authentication.Token;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Web.RouteResolvers;

namespace NuFridge.Shared.Server.Web
{
    public class WebBootstrapper : AutofacNancyBootstrapper, IPortalBootstrapper
    {
        private readonly ILifetimeScope _container;
        private readonly Lazy<IWebPortalConfiguration> _portalConfiguration;
        private readonly ILog _log = LogProvider.For<WebBootstrapper>();

        public ServerState State { get; set; }

        public string StatusMessage { get; set; }

        protected override byte[] FavIcon
        {
            get
            {
                return null;
            }
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(builder =>
                {
                    builder.StatusCodeHandlers = new List<Type>
            {
                typeof (ErrorStatusCodeHandler)
            };
                    builder.Serializers.Clear();
                    builder.Serializers.Add(typeof(JsonNetSerializer));
                    builder.Serializers.Add(typeof(DefaultXmlSerializer));
                    builder.RouteResolver = typeof(LazyRouteResolver);
                });
            }
        }

        public WebBootstrapper(ILifetimeScope container, Lazy<IWebPortalConfiguration> portalConfiguration)
        {
            _container = container;
            _portalConfiguration = portalConfiguration;
        }

        protected override ILifetimeScope GetApplicationContainer()
        {
            return _container;
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            Assembly assembly = Assembly.LoadFile(Path.Combine(Directory.GetParent(System.Reflection.Assembly.GetEntryAssembly().Location).FullName, "NuFridge.Website.dll"));
            string resourceNamespaceRoot = "NuFridge.Website";
            nancyConventions.StaticContentsConventions.Add(ResponseDecorator.StaticContent(EmbeddedStaticContentConventionBuilder.MapVirtualDirectory("", resourceNamespaceRoot, assembly), _portalConfiguration.Value));
            nancyConventions.StaticContentsConventions.Add(ResponseDecorator.StaticContent(EmbeddedStaticContentConventionBuilder.MapFile("/", resourceNamespaceRoot + ".index.html", assembly), _portalConfiguration.Value));
        }

        protected override void ApplicationStartup(ILifetimeScope applicationContainer, IPipelines pipelines)
        {
            base.ApplicationStartup(_container, pipelines);

            DiagnosticsHook.Disable(pipelines);
        }

        protected override void ConfigureRequestContainer(ILifetimeScope requestScope, NancyContext context)
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();

            containerBuilder.Update(requestScope.ComponentRegistry);
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            var tokenConfig = new TokenAuthenticationConfiguration(container.Resolve<ITokenizer>());
 
            TokenAuthentication.Enable(pipelines, tokenConfig);

            pipelines.BeforeRequest.AddItemToEndOfPipeline(nancyContext =>
            {
                _log.TraceFormat("{0} {1}", nancyContext.Request.Method.PadRight(5, ' '), nancyContext.Request.Url);
                return (Response)null;
            });

            base.RequestStartup(container, pipelines, context);
        }
    }
}
