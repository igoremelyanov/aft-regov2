using System;
using System.Threading.Tasks;

namespace AFT.RegoV2.Shared.Helpers
{
    public static class WaitHelper
    {
        /// <summary>
        /// Waits until result of func is not null
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func">Function to get result to wait. If result of the function is not null waiting stops.</param>
        /// <param name="timeout">Time in milliseconds before the timeout</param>
        /// <param name="delay">Delay between action calls in milliseconds</param>
        /// <returns></returns>
        public static async Task<TResult> WaitResultAsync<TResult>(Func<TResult> func, int timeout = 30*1000, int delay = 500) where TResult : class
        {
            var stopDate = DateTime.Now.AddMilliseconds(timeout);
            TResult result;

            do
            {
                result = func();

                if (result != null)
                {
                    return result;
                }

                await Task.Delay(delay);
            }
            while (DateTime.Now < stopDate);

            if (result == null)
            {
                throw new RegoException("Timeout of waiting for result");
            }

            return result;
        }
    }
}
