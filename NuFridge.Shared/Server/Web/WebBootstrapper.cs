using System.Diagnostics;
using Autofac;
using Nancy;
using Nancy.Authentication.Token;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Diagnostics;
using Nancy.Owin;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Web.Nancy;
using NuFridge.Shared.Server.Web.RouteResolvers;

namespace NuFridge.Shared.Server.Web
{
    public class WebBootstrapper : AutofacNancyBootstrapper, IPortalBootstrapper
    {
        private readonly ILifetimeScope _container;
        private readonly ILog _log = LogProvider.For<WebBootstrapper>();


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
                    builder.Serializers.Clear();
                    builder.Serializers.Add(typeof(JsonNetSerializer));
                    builder.Serializers.Add(typeof(DefaultXmlSerializer));
                    builder.RouteResolver = typeof(LazyRouteResolver);
                });
            }
        }

        public WebBootstrapper(ILifetimeScope container)
        {
            _container = container;
        }

        protected override ILifetimeScope GetApplicationContainer()
        {
            return _container;
        }

        protected override void ApplicationStartup(ILifetimeScope applicationContainer, IPipelines pipelines)
        {
            pipelines.BeforeRequest += StopWatchStart;
            pipelines.AfterRequest += StopWatchStop;

            DiagnosticsHook.Disable(pipelines);
            StaticConfiguration.DisableErrorTraces = false;
        }

        private Response StopWatchStart(NancyContext ctx)
        {
            var stopwatch = new Stopwatch();
            ctx.Items["stopwatch"] = stopwatch;
            stopwatch.Start();
            return null;
        }

        private void StopWatchStop(NancyContext ctx)
        {
            if (ctx.Items.ContainsKey("stopwatch"))
            {
                var stopwatch = (Stopwatch)ctx.Items["stopwatch"];
                stopwatch.Stop();
                _log.Trace(ctx.Request.Method + "   " + ctx.Request.Url + "   " + stopwatch.ElapsedMilliseconds + "ms");
            }
        }

        protected override void ConfigureRequestContainer(ILifetimeScope requestScope, NancyContext context)
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.Register<ICurrentRequest>((c, o) => new CurrentRequest(context));
            containerBuilder.Update(requestScope.ComponentRegistry);
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            var tokenConfig = new TokenAuthenticationConfiguration(container.Resolve<ITokenizer>());

            TokenAuthentication.Enable(pipelines, tokenConfig);

            pipelines.OnError.AddItemToEndOfPipeline((z, a) =>
            {
                _log.ErrorException("Unhandled error on request: " + context.Request.Url + " : " + a.ToString(), a);
                return context.Response;
            });

            base.RequestStartup(container, pipelines, context);
        }
    }
}