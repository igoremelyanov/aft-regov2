using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Core.Game.Interface.Data;

namespace AFT.RegoV2.Core.Game.Entities
{
    public class Wallet
    {
        private readonly Data.Wallet _data;
        private readonly List<IDomainEvent> _events;

        internal Data.Wallet Data { get { return _data; } }
        internal IEnumerable<IDomainEvent> Events { get { return _events; } }

        public Wallet(Data.Wallet wallet)
        {
            _data = wallet;
            _events = new List<IDomainEvent>();
        }
        
        public Transaction FundIn(decimal amount, string transactionNumber)
        {
            ValidateOperationAmount(amount);
            _data.Balance += amount;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.UgsFundIn,
                MainBalanceAmount = amount,
                TransactionNumber = transactionNumber,
            };

            transaction.Description = "Fund In, " + amount.ToString("F") + " ";
            
            AddTransaction(transaction);

            return transaction;
        }
        
        public Transaction FundOut(decimal amount, string transactionNumber)
        {
            ValidateOperationAmount(amount);

            if (_data.Balance < amount)
                throw new InsufficientFundsException();

            _data.Balance -= amount;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.UgsFundOut,
                MainBalanceAmount = amount,
                TransactionNumber = transactionNumber,
            };

            transaction.Description = "Fund Out, " + amount.ToString("F") + " ";
            
            AddTransaction(transaction);

            return transaction;
        }

        
        public Transaction PlaceBet(decimal amount, Guid roundId, Guid gameId, string externalTransactionId)
        {
            ValidateOperationAmount(amount);
            if (_data.Balance < amount) throw new InsufficientFundsException();

            decimal mainBalanceDebit;
            if (_data.Balance >= amount)
            {
                mainBalanceDebit = -amount;
            }
            else
            {
                mainBalanceDebit = -_data.Balance;
            }

            _data.Balance += mainBalanceDebit;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetPlaced,
                MainBalanceAmount = mainBalanceDebit,
                GameId = gameId,
                RoundId = roundId,
                ExternalTransactionId = externalTransactionId
            };

            transaction.Description = "Game, Place Bet, " + amount.ToString("F") + " "; 
            
            AddTransaction(transaction);

            return transaction;
        }

        public Transaction FreeBet(decimal amount, Guid roundId, Guid gameId, string externalTransactionId)
        {
            ValidateOperationAmount(amount);
            
            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetFree,
                GameId = gameId,
                RoundId = roundId,
                ExternalTransactionId = externalTransactionId,
                Description = "Game, Free Bet, " + amount.ToString("F") + " "
            };

            transaction.MainBalanceAmount = amount;
            _data.Balance += amount;

            AddTransaction(transaction);

            return transaction;
        }

        public Transaction WinBet(Guid roundId, decimal amount, string externalTransactionId)
        {
            ValidateOperationAmount(amount);
            var betPlacedTransactions = GetBetPlacedTransactions(roundId);

            _data.Balance += amount;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetWon,
                GameId = betPlacedTransactions.First().GameId,
                RoundId = roundId,
                MainBalance = _data.Balance,
                MainBalanceAmount = amount,
                ExternalTransactionId = externalTransactionId,
                Description = "Game, Win Bet, " + amount.ToString("F") + " "
            };

            AddTransaction(transaction);

            return transaction;
        }

        public Transaction TieBet(Guid roundId, decimal amount, string externalTransactionId)
        {
            ValidateOperationAmount(amount);
            var betPlacedTransactions = GetBetPlacedTransactions(roundId);

            _data.Balance += amount;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetTied,
                GameId = betPlacedTransactions.First().GameId,
                MainBalance = _data.Balance,
                MainBalanceAmount = amount,
                RoundId = roundId,
                ExternalTransactionId = externalTransactionId,
                Description = "Game, Tie Bet, " + amount.ToString("F") + " "
            };

            AddTransaction(transaction);

            return transaction;
        }

        public Transaction LoseBet(Guid roundId, string externalTransactionId)
        {
            var betPlacedTransactions = GetBetPlacedTransactions(roundId);

            var amount = betPlacedTransactions.Average(tr => tr.MainBalanceAmount);

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetLost,
                MainBalanceAmount = amount,
                GameId = betPlacedTransactions.First().GameId,
                RoundId = roundId,
                ExternalTransactionId = externalTransactionId,
                Description = "Game, Lose Bet, " + amount.ToString("F") + " "
            };

            AddTransaction(transaction);

            return transaction;
        }
        public Transaction CancelBet(Guid roundId, string externalTransactionId, string externalTransactionReferenceId)
        {
            var trxToCancel = _data.Transactions.SingleOrDefault(tr => 
                tr.ExternalTransactionId == externalTransactionReferenceId &&
                tr.RoundId == roundId);
            if (trxToCancel == null)
            {
                throw new RegoException("Bet with such transactionId does not exist.");
            }
            var trxType = trxToCancel.Type;
            if (trxType != TransactionType.BetPlaced && trxType != TransactionType.BetWon &&
                trxType != TransactionType.BetLost)
            {
                throw new InvalidOperationException("Transaction type is not supported.");
            }
            var duplicateCancellation = _data.Transactions.Any(tr =>
                    tr.Type == TransactionType.BetCancelled &&
                    tr.RelatedTransactionId == trxToCancel.Id);
            if (duplicateCancellation)
            {
                throw new InvalidOperationException("Bet was already canceled.");
            }

            _data.Balance -= trxToCancel.MainBalanceAmount;

            var transaction = new Transaction(_data)
            {
                Type = TransactionType.BetCancelled,
                GameId = trxToCancel.GameId,
                RoundId = trxToCancel.RoundId,
                RelatedTransactionId = trxToCancel.Id,
                MainBalanceAmount = -trxToCancel.MainBalanceAmount,
                ExternalTransactionId = externalTransactionId,
                ExternalTransactionReferenceId = externalTransactionReferenceId
            };

            var amount = trxToCancel.MainBalanceAmount;
            transaction.Description = "Game, Cancel Bet, " + amount.ToString("F") + " ";

            AddTransaction(transaction);

            return transaction;
        }


        private void AddTransaction(Transaction transaction)
        {
            _data.Transactions.Add(transaction);

        }


        
        private void ValidateOperationAmount(decimal amount)
        {
            if (amount <= 0)
                throw new InvalidAmountException();
        }

        private List<Transaction> GetBetPlacedTransactions(Guid roundId)
        {
            var betPlacedTransactions =
                _data.Transactions.Where(tr => tr.RoundId == roundId && tr.Type == TransactionType.BetPlaced).ToList();
            if (betPlacedTransactions.Any() == false)
            {
                Trace.WriteLine("No bets were placed with this roundId.");
                throw new RegoException("No bets were placed with this roundId.");
            }

            return betPlacedTransactions;
        }
    }
}