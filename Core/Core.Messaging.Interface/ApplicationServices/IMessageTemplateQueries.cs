using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Messaging.Interface.ApplicationServices
{
    public interface IMessageTemplateQueries : IApplicationService
    {
        IQueryable<MessageTemplate> GetMessageTemplates();
        IEnumerable<Language> GetBrandLanguages(Guid id);
        IEnumerable<MessageType> GetBonusNotificationTriggerMessageTypes();
        ValidationResult GetValidationResult(AddMessageTemplate model);
        ValidationResult GetValidationResult(EditMessageTemplate model);
        ValidationResult GetValidationResult(ActivateMessageTemplate model);
    }
}