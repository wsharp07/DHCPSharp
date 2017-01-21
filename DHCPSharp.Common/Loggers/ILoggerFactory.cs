using System;

namespace DHCPSharp.Common.Loggers
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(string name);
        ILogger GetClassLogger(Type typeToLog);
    }
}
