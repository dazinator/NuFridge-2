using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception UnpackFromContainers(this Exception error)
        {
            AggregateException aggregateException = error as AggregateException;
            if (aggregateException != null && aggregateException.InnerExceptions.Count == 1)
                return aggregateException.InnerExceptions[0].UnpackFromContainers();
            if (error is TargetInvocationException && error.InnerException != null)
                return error.InnerException.UnpackFromContainers();
            return error;
        }

        public static string SuggestUrlReservations(IList<Uri> prefixes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("The HTTP server could not start because namespace reservations have not been made for the current user. ");
            stringBuilder.AppendLine("The following command will give the user access to the urls requested:");
            foreach (Uri uri in prefixes)
                stringBuilder.AppendFormat("  netsh http add urlacl url={0}://+:{1}{2} user={3}\\{4}", uri.Scheme, uri.Port, uri.PathAndQuery, Environment.UserDomainName, Environment.UserName).AppendLine();
            return stringBuilder.ToString();
        }

        public static string SuggestSolution(this HttpListenerException error, IList<Uri> prefixes)
        {
            if (error.ErrorCode != 5)
                return null;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("The service was unable to start because there was a problem starting the website: {0}", error.Message).AppendLine();
            stringBuilder.AppendLine("Check that the current user has access to listen on these prefixes by running the following command:");
            foreach (Uri uri in prefixes)
                stringBuilder.AppendFormat("  netsh http add urlacl url={0}://+:{1}{2} user={3}\\{4}", uri.Scheme, uri.Port, uri.PathAndQuery, Environment.UserDomainName, Environment.UserName).AppendLine();
            stringBuilder.Append("Check that no other processes are listening on the same port: ").Append(string.Join(", ", prefixes.Select(p => p.Port)));
            return stringBuilder.ToString();
        }

        public static string GetErrorSummary(this Exception error)
        {
            error = error.UnpackFromContainers();
            if (error is TaskCanceledException)
                return string.Empty;
            return error.Message;
        }
    }
}
