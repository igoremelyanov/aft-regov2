using System.Transactions;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace AFT.RegoV2.Shared.Utils
{
    public class CustomTransactionScope
    {
        public static TransactionScope GetTransactionSuppressedScope()
        {
            return new TransactionScope(TransactionScopeOption.Suppress,
                new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.Unspecified,
                    Timeout = TransactionManager.MaximumTimeout
                });
        }

        public static TransactionScope GetTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = isolationLevel,
                    Timeout = TransactionManager.MaximumTimeout
                });
        }

        public static TransactionScope GetTransactionScopeAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = isolationLevel,
                    Timeout = TransactionManager.MaximumTimeout
                }, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}