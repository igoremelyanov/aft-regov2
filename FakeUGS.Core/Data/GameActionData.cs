using System;

namespace FakeUGS.Core.Data
{
    public class GameActionData
    {
        public string RoundId { get; set; }
        public string ExternalGameId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string ExternalBetId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string TransactionReferenceId { get; set; }
        public string BatchId { get; set; }

        public static GameActionData NewGameActionData(string roundId, 
            decimal amount, 
            string currencyCode,
            string externalGameId = null,
            string externalTransactionId = null,
            string description = null,
            string transactionReferenceId = null)
        {
            return new GameActionData
            {
                RoundId = roundId,
                ExternalGameId = externalGameId,
                Amount = amount,
                CurrencyCode = currencyCode,
                ExternalTransactionId = externalTransactionId ?? Guid.NewGuid().ToString(),
                TransactionReferenceId = transactionReferenceId,
                Description = description ?? String.Empty
            };
        }

    }
}
