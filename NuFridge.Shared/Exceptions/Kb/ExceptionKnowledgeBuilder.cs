using System;
using System.Collections.Generic;
using System.Linq;

namespace NuFridge.Shared.Exceptions.Kb
{
    internal class ExceptionKnowledgeBuilder
    {
        private readonly List<Func<Exception, IDictionary<string, object>, bool>> _clauses = new List<Func<Exception, IDictionary<string, object>, bool>>();
        private Func<IDictionary<string, object>, string> _entrySummary = s => (string)null;
        private Func<IDictionary<string, object>, string> _entryHelpText = s => (string)null;


        public ExceptionKnowledge Build()
        {
            return new ExceptionKnowledge(ex =>
            {
                if (!_clauses.Any())
                    return (ExceptionKnowledgeBaseEntry)null;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                foreach (Func<Exception, IDictionary<string, object>, bool> func in _clauses)
                {
                    if (!func(ex, dictionary))
                        return (ExceptionKnowledgeBaseEntry)null;
                }
                string summary = _entrySummary(dictionary);
                if (summary == null)
                    return (ExceptionKnowledgeBaseEntry)null;
                return new ExceptionKnowledgeBaseEntry(summary, _entryHelpText(dictionary));
            });
        }

        public ExceptionKnowledgeBuilder ExceptionIs<T>() where T : Exception
        {
            return ExceptionIs(ex => true, (Action<T, IDictionary<string, object>>)((param0, param1) => { }));
        }

        public ExceptionKnowledgeBuilder ExceptionIs<T>(Func<T, bool> predicate) where T : Exception
        {
            return ExceptionIs(predicate, (param0, param1) => { });
        }

        public ExceptionKnowledgeBuilder ExceptionIs<T>(Action<T, IDictionary<string, object>> getState) where T : Exception
        {
            return ExceptionIs(ex => true, getState);
        }

        public ExceptionKnowledgeBuilder ExceptionIs<T>(Func<T, bool> predicate, Action<T, IDictionary<string, object>> getState) where T : Exception
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (getState == null)
                throw new ArgumentNullException("getState");
            _clauses.Add((ex, s) =>
            {
                T obj = ex as T;
                if (obj == null || !predicate(obj))
                    return false;
                getState(obj, s);
                return true;
            });
            return this;
        }

        public ExceptionKnowledgeBuilder HasInnerException<T>() where T : Exception
        {
            return HasInnerException(ex => true, (Action<T, IDictionary<string, object>>)((param0, param1) => { }));
        }

        public ExceptionKnowledgeBuilder HasInnerException<T>(Func<T, bool> predicate) where T : Exception
        {
            return HasInnerException(predicate, (param0, param1) => { });
        }

        public ExceptionKnowledgeBuilder HasInnerException<T>(Action<T, IDictionary<string, object>> getState) where T : Exception
        {
            return HasInnerException(ex => true, getState);
        }

        public ExceptionKnowledgeBuilder HasInnerException<T>(Func<T, bool> predicate, Action<T, IDictionary<string, object>> getState) where T : Exception
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            if (getState == null)
                throw new ArgumentNullException("getState");
            _clauses.Add((ex, s) =>
            {
                if (ex.InnerException == null)
                    return false;
                foreach (Exception exception in Enumerate(ex.InnerException))
                {
                    T obj = exception as T;
                    if (obj != null && predicate(obj))
                    {
                        getState(obj, s);
                        return true;
                    }
                }
                return false;
            });
            return this;
        }

        private IEnumerable<Exception> Enumerate(Exception exception)
        {
            yield return exception;
            AggregateException ag = exception as AggregateException;
            if (ag != null)
            {
                foreach (Exception exception1 in ag.InnerExceptions)
                {
                    foreach (Exception exception2 in Enumerate(exception1))
                        yield return exception2;
                }
            }
            else if (exception.InnerException != null)
            {
                foreach (Exception exception1 in Enumerate(exception.InnerException))
                    yield return exception1;
            }
        }

        public ExceptionKnowledgeBuilder EntrySummaryIs(string summary)
        {
            return EntrySummaryIs(s => summary);
        }

        public ExceptionKnowledgeBuilder EntrySummaryIs(Func<IDictionary<string, object>, string> getSummary)
        {
            _entrySummary = getSummary;
            return this;
        }

        public ExceptionKnowledgeBuilder EntryHelpTextIs(string help)
        {
            return EntryHelpTextIs(s => help);
        }

        public ExceptionKnowledgeBuilder EntryHelpTextIs(Func<IDictionary<string, object>, string> getHelp)
        {
            _entryHelpText = getHelp;
            return this;
        }
    }
}
