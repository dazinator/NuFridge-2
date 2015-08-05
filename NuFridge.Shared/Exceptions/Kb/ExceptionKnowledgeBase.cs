using System;
using System.Collections.Generic;
using System.Net;
using NuFridge.Shared.Extensions;

namespace NuFridge.Shared.Exceptions.Kb
{
    public static class ExceptionKnowledgeBase
    {
        private static readonly List<ExceptionKnowledge> Rules = new List<ExceptionKnowledge>();

        static ExceptionKnowledgeBase()
        {
            AddRule(
                r =>
                    r.ExceptionIs(ex => ex.Message.StartsWith("Could not connect to the feed"),
                        (Action<InvalidOperationException, IDictionary<string, object>>)
                            ((ex, s) => s["Generic"] =  ex.Message))
                        .EntrySummaryIs(
                            s =>  s["Generic"] +  " The feed responded with: " +  s["Client"]));

            AddRule(r => r.ExceptionIs((Func<HttpListenerException, bool>) (ex =>
            {
                if (
                    ex.Message.StartsWith(
                        "The process cannot access the file because it is being used by another process"))
                    return ex.StackTrace.Contains("System.Net.HttpListener.Start()");
                return false;
            }))
                .EntrySummaryIs("The website failed to start because the port is in use.")
                .EntryHelpTextIs("The required port or URL is being used by another process."));
        }

        private static void AddRule(Action<ExceptionKnowledgeBuilder> buildRule)
        {
            ExceptionKnowledgeBuilder knowledgeBuilder = new ExceptionKnowledgeBuilder();
            buildRule(knowledgeBuilder);
            Rules.Add(knowledgeBuilder.Build());
        }

        public static bool TryInterpret(Exception exception, out ExceptionKnowledgeBaseEntry entry)
        {
            Exception exception1 = exception.UnpackFromContainers();
            try
            {
                foreach (ExceptionKnowledge exceptionKnowledge in Rules)
                {
                    if (exceptionKnowledge.TryMatch(exception1, out entry))
                        return true;
                }
            }
            catch
            {
            }
            entry = null;
            return false;
        }
    }
}
