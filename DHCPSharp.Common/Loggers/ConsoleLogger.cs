using System;

namespace DHCPSharp.Common.Loggers
{
    public class ConsoleLogger : ILogger
    {
        private readonly string PREFIX_FORMAT = "[{0}] [{1}] ";
        public bool IsDebugEnabled => true;

        public void Debug(object message)
        {
            var prefix = GetPrefix("debug");
            Console.WriteLine(prefix + message);
        }

        public void Debug(object message, Exception exception)
        {
            Debug(message + Environment.NewLine + $"Exception: {exception.Message}");
        }

        public void DebugFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(object message)
        {
            var prefix = GetPrefix("error");
            Console.WriteLine(prefix + message);
        }

        public void Error(Exception exception, object message)
        {
            Error(message + Environment.NewLine + $"Exception: {exception.Message}");
        }

        public void ErrorFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Fatal(object message)
        {
            var prefix = GetPrefix("fatal");
            Console.WriteLine(prefix + message);
        }

        public void Fatal(object message, Exception exception)
        {
            Fatal(message + Environment.NewLine + $"Exception: {exception.Message}");
        }

        public void FatalFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(object message)
        {
            var prefix = GetPrefix("info");
            Console.WriteLine(prefix + message);
        }

        public void Info(object message, Exception exception)
        {
            Info(message + Environment.NewLine + $"Exception: {exception.Message}");
        }

        public void InfoFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(object message)
        {
            var prefix = GetPrefix("warn");
            Console.WriteLine(prefix + message);
        }

        public void Warn(object message, Exception exception)
        {
            Warn(message + Environment.NewLine + $"Exception: {exception.Message}");
        }

        public void WarnFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        private string GetPrefix(string logLevel)
        {
            var prefix = String.Format(PREFIX_FORMAT, logLevel.ToUpper(), DateTime.Now.ToString("yyyy-MM-dd hh:mmm:ss"));
            return prefix;
        }
    }
}
