using System;

using AFT.RegoV2.Shared.Logging;

using NLog;

namespace AFT.RegoV2.Shared.Logging
{
    public class LogDecorator : ILog
    {
        private readonly Logger _logger;

        public LogDecorator()
        {
            _logger = LogManager.GetLogger("DefaultLogger");
        }

        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }
    }
}
