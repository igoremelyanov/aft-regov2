using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateCommands;

namespace AFT.RegoV2.Core.Messaging.Interface.ApplicationServices
{
    public interface IMessageTemplateCommands : IApplicationService
    {
        Guid Add(AddMessageTemplate model);
        void Edit(EditMessageTemplate model);
        void Activate(ActivateMessageTemplate model);
    }
}