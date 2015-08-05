using System.Collections.Generic;
using System.Linq;

namespace NuFridge.Shared.Exceptions.Kb
{
    public class ExceptionKnowledgeBaseEntry
    {
        public string Summary { get; }

        public string HelpText { get; private set; }

        public ExceptionKnowledgeBaseEntry(string summary, string helpText)
        {
            HelpText = helpText;
            Summary = summary;
        }

        public override string ToString()
        {
            List<string> list = new string[1]
            {
                Summary
            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return string.Join(" ", list);
        }
    }
}
