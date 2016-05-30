using System;
using System.Threading.Tasks;

namespace AFT.RegoV2.Shared.Synchronization
{
    public interface ISynchronizationService
    {
        void Execute(string sectionName, Action action);
        Task ExecuteAsync(string sectionName, Func<Task> action);
    }
}
