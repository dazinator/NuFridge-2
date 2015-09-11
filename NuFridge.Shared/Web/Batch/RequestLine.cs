namespace NuFridge.Shared.Web.Batch
{
    internal class RequestLine
    {
        public RequestLine(string method, string uri, string httpVersion)
        {
            Method = method;
            Uri = uri;
            HttpVersion = httpVersion;
        }

        public string HttpVersion { get; }

        public string Uri { get; }

        public string Method { get; }
    }
}
