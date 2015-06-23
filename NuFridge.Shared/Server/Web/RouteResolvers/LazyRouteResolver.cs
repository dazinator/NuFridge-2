using System;
using Nancy;
using Nancy.Routing;
using Nancy.Routing.Trie;

namespace NuFridge.Shared.Server.Web.RouteResolvers
{
    public class LazyRouteResolver : IRouteResolver
    {
        private readonly Lazy<IRouteResolver> _realResolver;

        public LazyRouteResolver(Lazy<INancyModuleCatalog> moduleCatalog, Lazy<INancyModuleBuilder> moduleBuilder, Lazy<IRouteCache> routeCache, Lazy<IRouteResolverTrie> routeResolverTrie)
        {
            _realResolver = new Lazy<IRouteResolver>(() => (IRouteResolver)new DefaultRouteResolver(moduleCatalog.Value, moduleBuilder.Value, routeCache.Value, routeResolverTrie.Value));
        }

        public ResolveResult Resolve(NancyContext context)
        {
            return _realResolver.Value.Resolve(context);
        }
    }
}
