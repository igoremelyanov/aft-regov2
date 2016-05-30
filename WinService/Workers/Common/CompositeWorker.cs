using System;
using System.Linq;
using System.Threading;

using AFT.RegoV2.Shared.Logging;

namespace WinService.Workers
{
    public class CompositeWorker : IWorker
    {
        private readonly Func<IWorker[]> _workersFactory;
        private IWorker[] _workers;
        private readonly ILog _logger;

        public CompositeWorker(
            Func<IWorker[]> workersFactory,
            ILog logger
            )
        {
            _workersFactory = workersFactory;
            _logger = logger;
        }

        private void StartService()
        {
            _workers = _workersFactory();
            _logger.Info(string.Format("Found {0} workers to be started.", _workers.Length));

            foreach (var worker in _workers)
            {
                worker.Start();
                _logger.Debug(worker.GetType().Name + " started.");
            }

            _logger.Info(string.Format("All {0} workers started successfully.", _workers.Length));
        }

        public void Start()
        {
            _logger.Debug("CompositeWorker is starting...");

            ThreadStart threadDelegate = StartService;
            var thread = new Thread(threadDelegate);
            thread.Start();
        }

        public void Stop()
        {
            _logger.Debug("CompositeWorker is stopping...");

            foreach (var worker in _workers)
            {
                worker.Stop();
                _logger.Info(worker.GetType().Name + " stopped.");
            }

            _logger.Info(string.Format("All {0} workers stopped successfully.", _workers.Length));
        }
    }
}