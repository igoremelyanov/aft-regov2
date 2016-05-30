using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;

namespace AFT.RegoV2.Core.Messaging.Interface.ApplicationServices
{
    public interface IMassMessageCommands : IApplicationService
    {
        UpdateRecipientsResponse UpdateRecipients(UpdateRecipientsRequest request);
        SendMassMessageResponse Send(SendMassMessageRequest request);
    }
}