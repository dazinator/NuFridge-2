using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Service.Hosting;
using NuFridge.Service.Logging;

namespace NuFridge.Service.Api
{
    public class ApiLogger
    {
        private static readonly ILog Logger = LogProvider.For<ApiLogger>();

        private readonly Func<IDictionary<string, object>, Task> _next;

        public ApiLogger(Func<IDictionary<string, object>, Task> next)
        {
            if (next == null)
                throw new ArgumentNullException("next");
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            string method = GetValueFromEnvironment(environment, OwinConstants.RequestMethod);
            string path = GetValueFromEnvironment(environment, OwinConstants.RequestPathBase) + GetValueFromEnvironment(environment, OwinConstants.RequestPath);

            Stopwatch stopWatch = Stopwatch.StartNew();
            return _next(environment).ContinueWith(t =>
            {
                Logger.Info(string.Format("{0}\t{1}\t{2}ms\t{3}\t{4}", method, path, stopWatch.ElapsedMilliseconds,
                  GetValueFromEnvironment(environment, OwinConstants.ResponseStatusCode),
                  GetValueFromEnvironment(environment, OwinConstants.ResponseReasonPhrase)));
                return t;
            });
        }

        private static string GetValueFromEnvironment(IDictionary<string, object> environment, string key)
        {
            object value;
            environment.TryGetValue(key, out value);
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}
