using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Game.Interface.Exceptions;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Extensions;
using AFT.RegoV2.GameApi.Interface.Classes;

namespace AFT.RegoV2.GameApi.Shared.Services
{
    public interface IErrorManager
    {
        GameApiErrorCode GetErrorCodeByException(Exception exception, out string description);
    }
    public sealed class ErrorManager : IErrorManager
    {
        private static readonly IDictionary<Type, GameApiErrorCode> CodeByExceptionType =
            new Dictionary<Type, GameApiErrorCode>
            {
                {typeof (InvalidTokenException), GameApiErrorCode.InvalidToken},
                {typeof (InsufficientFundsException), GameApiErrorCode.InsufficientFunds},
                {typeof (InvalidAmountException), GameApiErrorCode.IncorrectFormat},
                {typeof (GameActionNotFoundException), GameApiErrorCode.GameActionNotFound},
                {typeof (RoundNotFoundException), GameApiErrorCode.RoundNotFound},
                {typeof (DuplicateGameActionException), GameApiErrorCode.DuplicateGameActionId},
                {typeof (DuplicateBatchException), GameApiErrorCode.DuplicateBatchId},
                {typeof (InvalidVipLevelBetException), GameApiErrorCode.InvalidVipLevelBet},
                {typeof (ArgumentException), GameApiErrorCode.InvalidArguments},
                {typeof (LoseBetAmountMustBeZeroException), GameApiErrorCode.LoseBetAmountNotZero},
                {typeof (InvalidTransactionTypeException), GameApiErrorCode.InvalidSettleBetTransactionType},
                {typeof (FrozenAccountException), GameApiErrorCode.AccountIsFrozen}

            };
        
        GameApiErrorCode IErrorManager.GetErrorCodeByException(Exception exception, out string description)
        {
            var code = GameApiErrorCode.NoError;
            if (exception == null)
            {
                description = code.GetDescription();
                return code;
            }
            if (!CodeByExceptionType.TryGetValue(exception.GetType(), out code))
            {
                code = GameApiErrorCode.SystemError;
            }
            description = code.GetDescription() + " (" + exception.Message + ")";
            return code;
        }

    }
}