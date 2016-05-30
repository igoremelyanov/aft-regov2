﻿using System.ComponentModel;

namespace FakeUGS.Core.Classes
{
    public enum GameApiErrorCode
    {
        [Description("No Error")]
        NoError = 0,
        [Description("Invalid authentication token")]
        InvalidToken = 10,
        [Description("User is blocked")]
        UserBlocked = 20,
        [Description("Invalid VIP Level Bet")]
        InvalidVipLevelBet = 30,
        [Description("Insufficient funds to perform the operation")]
        InsufficientFunds = 100,
        [Description("Transaction not found")]
        GameActionNotFound = 200,
        [Description("Duplicate Game Action ID")]
        DuplicateGameActionId = 210,
        [Description("Duplicate Batch ID")]
        DuplicateBatchId = 220,
        [Description("Round not found")]
        RoundNotFound = 230,
        [Description("Invalid argument(s)")]
        InvalidArguments = 300,
        [Description("Lose bet amount must be zero")]
        LoseBetAmountNotZero = 310,
        [Description("Invalid settle bet transaction type")]
        InvalidSettleBetTransactionType = 320,
        [Description("Incorrect format")]
        IncorrectFormat = 500,
        [Description("Account is frozen")]
        AccountIsFrozen = 510,
        [Description("System error")]
        SystemError = 900,
    }
}