using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Factory;

namespace NuFridge.Service
{
    public static class Storage
    {
        public static List<string> Logs = new List<string>(); 
    }
    public class LogCaptureFactoryAdapter : AbstractCachingLoggerFactoryAdapter
    {

        protected override ILog CreateLogger(string name)
        {
            return new LogCapture();
        }
    }
    public class LogCapture : ILog
    {
        

        private void ProcessMessageCallBack(Action<FormatMessageHandler> formatMessageCallback)
        {
            formatMessageCallback(delegate(string format, object[] args)
            {
                var str = string.Format(format, args);
                ProcessMessageOutput(str);
                return str;
            });
        }

        private void ProcessMessageOutput(string message)
        {
           // Storage.Logs.Add(message);
        }

        private void Process()
        {
            //No op
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Debug(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            Process();
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Debug(Action<FormatMessageHandler> formatMessageCallback)
        {
            ProcessMessageCallBack(formatMessageCallback);
        }

        public void Debug(object message, Exception exception)
        {
            Process();
        }

        public void Debug(object message)
        {
            Process();
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Process();
        }

        public void DebugFormat(string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void DebugFormat(string format, params object[] args)
        {
            Process();
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Error(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            Process();
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Error(Action<FormatMessageHandler> formatMessageCallback)
        {
            ProcessMessageCallBack(formatMessageCallback);
        }

        public void Error(object message, Exception exception)
        {
            Process();
        }

        public void Error(object message)
        {
            Process();
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Process();
        }

        public void ErrorFormat(string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Process();
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Fatal(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            Process();
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Fatal(Action<FormatMessageHandler> formatMessageCallback)
        {
            ProcessMessageCallBack(formatMessageCallback);
        }

        public void Fatal(object message, Exception exception)
        {
            Process();
        }

        public void Fatal(object message)
        {
            Process();
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Process();
        }

        public void FatalFormat(string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void FatalFormat(string format, params object[] args)
        {
            Process();
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Info(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            Process();
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Info(Action<FormatMessageHandler> formatMessageCallback)
        {
            ProcessMessageCallBack(formatMessageCallback);
        }

        public void Info(object message, Exception exception)
        {
            Process();
        }

        public void Info(object message)
        {
            Process();
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Process();
        }

        public void InfoFormat(string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void InfoFormat(string format, params object[] args)
        {
            Process();
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsTraceEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Trace(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            Process();
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Trace(Action<FormatMessageHandler> formatMessageCallback)
        {
           // ProcessMessageCallBack(formatMessageCallback);
        }

        public void Trace(object message, Exception exception)
        {
            Process();
        }

        public void Trace(object message)
        {
            Process();
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void TraceFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Process();
        }

        public void TraceFormat(string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void TraceFormat(string format, params object[] args)
        {
            Process();
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Warn(IFormatProvider formatProvider, Action<FormatMessageHandler> formatMessageCallback)
        {
            Process();
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback, Exception exception)
        {
            Process();
        }

        public void Warn(Action<FormatMessageHandler> formatMessageCallback)
        {
            Process();
        }

        public void Warn(object message, Exception exception)
        {
            Process();
        }

        public void Warn(object message)
        {
            Process();
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Process();
        }

        public void WarnFormat(string format, Exception exception, params object[] args)
        {
            Process();
        }

        public void WarnFormat(string format, params object[] args)
        {
            Process();
        }
    }
}
