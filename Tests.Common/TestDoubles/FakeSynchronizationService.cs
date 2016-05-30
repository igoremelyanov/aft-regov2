using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AFT.RegoV2.Shared.Synchronization;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeSynchronizationService : ISynchronizationService
    {
        private SemaphoreSlim _semaphore;
        private Dictionary<string, SemaphoreSlim> _syncs;

        public FakeSynchronizationService()
        {
            _semaphore = new SemaphoreSlim(1);
            _syncs = new Dictionary<string, SemaphoreSlim>();
        }

        public void Execute(string sectionName, Action action)
        {
            EnsureSyncs(sectionName);

            try
            {
                _syncs[sectionName].Wait();
                action();
            }
            finally
            {
                _syncs[sectionName].Release();
            }
        }

        public async Task ExecuteAsync(string sectionName, Func<Task> action)
        {
            EnsureSyncs(sectionName);

            try
            {
                await _syncs[sectionName].WaitAsync();
                await action();
            }
            finally
            {
                _syncs[sectionName].Release();
            }
        }

        private void EnsureSyncs(string sectionName)
        {
            try
            {
                _semaphore.Wait();

                if (!_syncs.ContainsKey(sectionName))
                {
                    _syncs.Add(sectionName, new SemaphoreSlim(1));
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
