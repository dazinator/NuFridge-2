using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nancy;
using Nancy.Responses;

namespace NuFridge.Shared.Server.Web
{
    public class EmbeddedStaticContentConventionBuilder
    {
        private readonly Assembly _assembly;
        private readonly Dictionary<string, StaticResource> _responders;

        private EmbeddedStaticContentConventionBuilder(Assembly assembly, Dictionary<string, StaticResource> responders)
        {
            _assembly = assembly;
            _responders = responders;
        }

        public static Func<NancyContext, string, Response> MapVirtualDirectory(string virtualDirectory, string resourceNamespaceRoot, Assembly assemblyContainingResource)
        {
            Dictionary<string, StaticResource> responders = LoadResources(assemblyContainingResource, resourceNamespaceRoot, virtualDirectory);

            return new EmbeddedStaticContentConventionBuilder(assemblyContainingResource, responders).Respond;
        }

        public static Func<NancyContext, string, Response> MapFile(string virtualPath, string embeddedResourceName, Assembly assemblyContainingResource)
        {
            Dictionary<string, StaticResource> responders = new Dictionary<string, StaticResource>(StringComparer.OrdinalIgnoreCase);
            virtualPath = virtualPath.TrimStart('/').Replace("/", ".");
            StaticResource staticResource = new StaticResource
            {
                ManifestResourceName = embeddedResourceName,
                MimeType = MimeTypes.GetMimeType(embeddedResourceName)
            };
            responders.Add(virtualPath, staticResource);

            return new EmbeddedStaticContentConventionBuilder(assemblyContainingResource, responders).Respond;
        }

        private static Dictionary<string, StaticResource> LoadResources(Assembly assembly, string resourceNamespaceRoot, string virtualDirectory)
        {
            virtualDirectory = virtualDirectory.TrimStart('/', '.');
            Dictionary<string, StaticResource> dictionary = new Dictionary<string, StaticResource>(StringComparer.OrdinalIgnoreCase);
            foreach (string str in assembly.GetManifestResourceNames())
            {
                if (str.StartsWith(resourceNamespaceRoot, StringComparison.OrdinalIgnoreCase))
                {
                    string key = (virtualDirectory + str.Substring(resourceNamespaceRoot.Length).Replace("/", ".")).TrimStart('/', '.');
                    StaticResource staticResource = new StaticResource
                    {
                        ManifestResourceName = str,
                        MimeType = MimeTypes.GetMimeType(str)
                    };
                    dictionary.Add(key, staticResource);
                }
            }
            return dictionary;
        }

        private StaticResource GetResource(string virtualPath)
        {
            virtualPath = virtualPath.Trim('/').Replace("/", ".");
            StaticResource staticResource;
            if (!_responders.TryGetValue(virtualPath, out staticResource))
                return null;
            return staticResource;
        }

        private Response Respond(NancyContext nancyContext, string compositeRootPath)
        {
            ResponseBuilder response = new ResponseBuilder
            {
                Builder = this,
                Resource = GetResource(nancyContext.Request.Url.Path)
            };
            if (response.Resource == null)
                return null;
            Func<Stream> func = response.Stream;
            return new StreamResponse(func, response.Resource.MimeType).WithStatusCode((HttpStatusCode)200);
        }

        private class StaticResource
        {
            public string ManifestResourceName;
            public string MimeType;
        }

        private sealed class ResponseBuilder
        {
            public StaticResource Resource;
            public EmbeddedStaticContentConventionBuilder Builder;

            public Stream Stream()
            {
                return Builder._assembly.GetManifestResourceStream(Resource.ManifestResourceName);
            }
        }
    }
}
