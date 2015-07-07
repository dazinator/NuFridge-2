using System;

namespace NuFridge.Shared.Exceptions.Kb
{
    internal class ExceptionKnowledge
    {
        private readonly Func<Exception, ExceptionKnowledgeBaseEntry> _tryMatch;

        public ExceptionKnowledge(Func<Exception, ExceptionKnowledgeBaseEntry> tryMatch)
        {
            if (tryMatch == null)
                throw new ArgumentNullException("tryMatch");
            _tryMatch = tryMatch;
        }

        public bool TryMatch(Exception exception, out ExceptionKnowledgeBaseEntry entry)
        {
            ExceptionKnowledgeBaseEntry knowledgeBaseEntry = _tryMatch(exception);
            if (knowledgeBaseEntry == null)
            {
                entry = null;
                return false;
            }
            entry = knowledgeBaseEntry;
            return true;
        }
    }
}
