using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Cookies;
using Nancy.Extensions;
using Nancy.Helpers;
using Nancy.Hosting.Self;
using Nancy.IO;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Web.Nancy
{
    [Serializable]
    public class CustomNancyHost : IDisposable
    {
        private readonly IList<Uri> _baseUriList;
        private HttpListener _listener;
        private readonly INancyEngine _engine;
        private readonly HostConfiguration _configuration;
        private readonly INancyBootstrapper _bootstrapper;
        private readonly ILog _log = LogProvider.For<CustomNancyHost>();

        public AuthenticationSchemeSelector AuthenticationSchemeSelector { get; set; }

        public CustomNancyHost(string url)
        {
            _baseUriList = new[] {new Uri(url)};
        }

        public CustomNancyHost(params Uri[] baseUris)
            : this(NancyBootstrapperLocator.Bootstrapper, new HostConfiguration(), baseUris)
        {
        }

        public CustomNancyHost(HostConfiguration configuration, params Uri[] baseUris)
            : this(NancyBootstrapperLocator.Bootstrapper, configuration, baseUris)
        {
        }

        public CustomNancyHost(INancyBootstrapper bootstrapper, params Uri[] baseUris)
            : this(bootstrapper, new HostConfiguration(), baseUris)
        {
        }

        public CustomNancyHost(INancyBootstrapper bootstrapper, HostConfiguration configuration, params Uri[] baseUris)
        {
            _bootstrapper = bootstrapper;
            _configuration = configuration ?? new HostConfiguration();
            _baseUriList = baseUris;
            bootstrapper.Initialise();
            _engine = bootstrapper.GetEngine();
        }

        public CustomNancyHost(Uri baseUri, INancyBootstrapper bootstrapper)
            : this(bootstrapper, new HostConfiguration(), baseUri)
        {
        }

        public CustomNancyHost(Uri baseUri, INancyBootstrapper bootstrapper, HostConfiguration configuration)
            : this(bootstrapper, configuration, baseUri)
        {
        }

        public void Dispose()
        {
            Stop();
            _bootstrapper.Dispose();
        }

        public void Start()
        {
            StartListener();
            try
            {
                _listener.BeginGetContext(GotCallback, null);
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);
                _configuration.UnhandledExceptionCallback(ex);
                throw;
            }
        }

        private void StartListener()
        {
            if (TryStartListener())
                return;
            if (!_configuration.UrlReservations.CreateAutomatically)
                throw new AutomaticUrlReservationCreationFailureException(GetPrefixes(), GetUser());
            if (!TryAddUrlReservations())
                throw new InvalidOperationException("Unable to configure namespace reservation");
            if (!TryStartListener())
                throw new InvalidOperationException("Unable to start listener");
        }

        private bool TryStartListener()
        {
            try
            {
                _listener = new HttpListener();
                foreach (string uriPrefix in GetPrefixes())
                    _listener.Prefixes.Add(uriPrefix);
                _listener.AuthenticationSchemeSelectorDelegate = AuthenticationSchemeSelectorDelegate;
                _listener.Start();
                return true;
            }
            catch (HttpListenerException ex)
            {
                _log.ErrorException(ex.Message, ex);

                if (ex.ErrorCode == 5)
                    return false;
                throw;
            }
        }

        private AuthenticationSchemes AuthenticationSchemeSelectorDelegate(HttpListenerRequest httpRequest)
        {
            AuthenticationSchemeSelector authenticationSchemeSelector = AuthenticationSchemeSelector;
            if (authenticationSchemeSelector != null)
                return authenticationSchemeSelector(httpRequest);
            return AuthenticationSchemes.Anonymous;
        }

        private bool TryAddUrlReservations()
        {
            string user = GetUser();
            foreach (string str in GetPrefixes())
            {
                if (!NetSh.AddUrlAcl(str, user))
                    return false;
            }
            return true;
        }

        private string GetUser()
        {
            if (!string.IsNullOrWhiteSpace(_configuration.UrlReservations.User))
                return _configuration.UrlReservations.User;
            return WindowsIdentity.GetCurrent().Name;
        }

        public void Stop()
        {
            if (!_listener.IsListening)
                return;
            _listener.Stop();
        }

        private IEnumerable<string> GetPrefixes()
        {
            foreach (Uri uri in _baseUriList)
            {
                string prefix = uri.ToString();
                if (_configuration.RewriteLocalhost && !uri.Host.Contains("."))
                    prefix = prefix.Replace("localhost", "+");
                yield return prefix;
            }
        }

        private Request ConvertRequestToNancyRequest(HttpListenerRequest request)
        {
            Uri uri1 = _baseUriList.FirstOrDefault(uri => uri.IsCaseInsensitiveBaseOf(request.Url));
            if (uri1 == null)
                throw new InvalidOperationException(string.Format("Unable to locate base URI for request: {0}", request.Url));
            long expectedRequestLength = GetExpectedRequestLength(request.Headers.ToDictionary());
            string str = uri1.MakeAppLocalPath(request.Url);
            Url url1 = new Url();
            url1.Scheme = (request.Url.Scheme);
            url1.HostName = (request.Url.Host);
            url1.Port = (request.Url.IsDefaultPort ? new int?() : request.Url.Port);
            url1.BasePath = (uri1.AbsolutePath.TrimEnd('/'));
            url1.Path = (HttpUtility.UrlDecode(str));
            url1.Query = (request.Url.Query);
            Url url2 = url1;
            byte[] numArray = null;
            if (_configuration.EnableClientCertificates)
            {
                X509Certificate2 clientCertificate = request.GetClientCertificate();
                if (clientCertificate != null)
                    numArray = clientCertificate.RawData;
            }
            return new Request(request.HttpMethod, url2, RequestStream.FromStream(request.InputStream, expectedRequestLength, false), request.Headers.ToDictionary(), request.RemoteEndPoint != null ? request.RemoteEndPoint.Address.ToString() : null, numArray);
        }

        private void ConvertNancyResponseToResponse(NancyContext nancyRequest, Response nancyResponse, HttpListenerResponse response)
        {
            foreach (KeyValuePair<string, string> keyValuePair in nancyResponse.Headers)
                response.AddHeader(keyValuePair.Key, keyValuePair.Value);
            using (IEnumerator<INancyCookie> enumerator = nancyResponse.Cookies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    INancyCookie current = enumerator.Current;
                    response.Headers.Add(HttpResponseHeader.SetCookie, current.ToString());
                }
            }
            if (nancyResponse.ReasonPhrase != null)
                response.StatusDescription = nancyResponse.ReasonPhrase;
            if (nancyResponse.ContentType != null)
                response.ContentType = nancyResponse.ContentType;
            response.StatusCode = (int)nancyResponse.StatusCode;
            if (_configuration.AllowChunkedEncoding && !nancyResponse.Headers.ContainsKey("Content-Length"))
                OutputWithDefaultTransferEncoding(nancyResponse, response);
            else
                OutputWithContentLength(nancyResponse, response);
            nancyRequest.Dispose();
        }

        private static void OutputWithDefaultTransferEncoding(Response nancyResponse, HttpListenerResponse response)
        {
            using (Stream outputStream = response.OutputStream)
                nancyResponse.Contents(outputStream);
        }

        private static void OutputWithContentLength(Response nancyResponse, HttpListenerResponse response)
        {
            byte[] buffer;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                nancyResponse.Contents(memoryStream);
                buffer = memoryStream.ToArray();
            }
            long num = nancyResponse.Headers.ContainsKey("Content-Length") ? Convert.ToInt64(nancyResponse.Headers["Content-Length"]) : buffer.Length;
            response.SendChunked = false;
            response.ContentLength64 = num;
            using (Stream outputStream = response.OutputStream)
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(outputStream))
                {
                    binaryWriter.Write(buffer);
                    binaryWriter.Flush();
                }
            }
        }

        private static long GetExpectedRequestLength(IDictionary<string, IEnumerable<string>> incomingHeaders)
        {
            if (incomingHeaders == null || !incomingHeaders.ContainsKey("Content-Length"))
                return 0L;
            string s = incomingHeaders["Content-Length"].SingleOrDefault();
            long result;
            if (s == null || !long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return 0L;
            return result;
        }

        private void GotCallback(IAsyncResult ar)
        {
            try
            {
                HttpListenerContext context = _listener.EndGetContext(ar);
                _listener.BeginGetContext(GotCallback, null);
                Process(context);
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                _configuration.UnhandledExceptionCallback(ex);
                try
                {
                    _listener.BeginGetContext(GotCallback, null);
                }
                catch
                {
                    _configuration.UnhandledExceptionCallback(ex);
                }
            }
        }

        private void Process(HttpListenerContext ctx)
        {
            try
            {
                _engine.HandleRequest(ConvertRequestToNancyRequest(ctx.Request), c =>
                {
                    c.Items["RequestPrincipal"] = ctx.User;
                    return c;
                }, c => ConvertNancyResponseToResponse(c, c.Response, ctx.Response), e => _configuration.UnhandledExceptionCallback(e), CancellationToken.None);
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                _configuration.UnhandledExceptionCallback(ex);
            }
        }
    }
}
