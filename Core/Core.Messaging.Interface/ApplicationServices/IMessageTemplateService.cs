using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MessageTemplateModels;

namespace AFT.RegoV2.Core.Messaging.Interface.ApplicationServices
{
    public interface IMessageTemplateService : IApplicationService
    {
        void TrySendPlayerMessage(
            Guid playerId,
            MessageType messageType,
            MessageDeliveryMethod messageDeliveryMethod,
            IPlayerMessageTemplateModel model,
            bool ignoreAccountSetting = false);

        void TrySendBrandEmail(
            string recipientName,
            string recipientEmail,
            Guid brandId,
            MessageType messageType,
            IBrandMessageTemplateModel model,
            string languageCode = null);

        void TrySendBrandSms(
            string recipientName,
            string recipientNumber,
            Guid brandId,
            MessageType messageType,
            IBrandMessageTemplateModel model,
            string languageCode = null);

        string ParseTemplate(string content, IMessageTemplateModel model);
    }
}