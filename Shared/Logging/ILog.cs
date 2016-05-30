using System;

namespace AFT.RegoV2.Shared.Logging
{
    public interface ILog
    {
        void Trace(string message);

        void Debug(string message);

        void Info(string message);

        void Warn(string message);

        void Error(string message);
        void Error(string message, Exception ex);
    }
}
