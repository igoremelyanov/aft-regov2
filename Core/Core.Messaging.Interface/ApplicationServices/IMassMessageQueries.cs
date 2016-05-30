using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Messaging.Interface.ApplicationServices
{
    public interface IMassMessageQueries : IApplicationService
    {
        IQueryable<Player> CreateMassMessagePlayerQuery(SearchPlayersRequest request);
        IQueryable<Player> GetRecipients(Guid massMessageId);
        ValidationResult GetValidationResult(UpdateRecipientsRequest request);
        ValidationResult GetValidationResult(SendMassMessageRequest request);
    }
}