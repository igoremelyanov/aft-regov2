using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Core.Messaging.Data;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;
using AFT.RegoV2.Core.Messaging.Interface.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Shared.Utils;
using AutoMapper;

namespace AFT.RegoV2.Core.Messaging.ApplicationServices
{
    public class MassMessageCommands : MarshalByRefObject, IMassMessageCommands
    {
        private readonly IMessagingRepository _messagingRepository;
        private readonly IMassMessageQueries _massMessageQueries;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;

        static MassMessageCommands()
        {
            Mapper.CreateMap<Language, Interface.Data.Language>();
        }

        public MassMessageCommands(
            IMessagingRepository messagingRepository,
            IMassMessageQueries massMessageQueries,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus)
        {
            _messagingRepository = messagingRepository;
            _massMessageQueries = massMessageQueries;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
        }

        [Permission(Permissions.Send, Module = Modules.MassMessageTool)]
        public UpdateRecipientsResponse UpdateRecipients(UpdateRecipientsRequest request)
        {
            var validationResult = _massMessageQueries.GetValidationResult(request);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                if (!request.Id.HasValue)
                    DeleteUnsentMassMessages();

                MassMessage massMessage;

                if (request.Id.HasValue)
                {
                    massMessage = _messagingRepository.MassMessages
                        .Include(x => x.Recipients.Select(y => y.Language))
                        .Single(x => x.Id == request.Id);
                }
                else
                {
                    massMessage = new MassMessage
                    {
                        Id = Guid.NewGuid(), 
                        AdminId = _actorInfoProvider.Actor.Id
                    };

                    _messagingRepository.MassMessages.Add(massMessage);
                }

                massMessage.IpAddress = request.IpAddress;

                if (request.UpdateRecipientsType == UpdateRecipientsType.SelectSingle)
                {
                    massMessage.Recipients.Add(_messagingRepository.Players
                        .Include(x => x.Language)
                        .Single(x => x.Id == request.PlayerId));
                }
                else if (request.UpdateRecipientsType == UpdateRecipientsType.UnselectSingle)
                {
                    massMessage.Recipients.Remove(massMessage.Recipients.Single(x => x.Id == request.PlayerId));
                }
                else if (request.UpdateRecipientsType == UpdateRecipientsType.RecipientsListUnselectAll)
                {
                    massMessage.Recipients.Clear();
                }
                else
                {
                    var playerIds = _massMessageQueries.CreateMassMessagePlayerQuery(request.SearchPlayersRequest)
                        .Select(x => x.Id);

                    if (request.UpdateRecipientsType == UpdateRecipientsType.SearchResultSelectAll)
                    {
                        var players = _messagingRepository.Players
                            .Include(x => x.Language)
                            .Where(x => playerIds.Contains(x.Id));

                        foreach (var player in players)
                        {
                            massMessage.Recipients.Add(player);
                        }
                    }
                    else
                    {
                        var recipients = massMessage.Recipients.Where(x => playerIds.Contains(x.Id)).ToArray();

                        foreach (var recipient in recipients)
                        {
                            massMessage.Recipients.Remove(recipient);
                        }
                    }
                }

                _messagingRepository.SaveChanges();

                scope.Complete();

                IEnumerable<Interface.Data.Language> languages = null;

                if (massMessage.Recipients.Any())
                {
                    var languageEntities = massMessage.Recipients.GroupBy(x => x.LanguageCode).Select(x => x.First().Language);
                    languages = Mapper.Map<List<Interface.Data.Language>>(languageEntities);
                }

                return new UpdateRecipientsResponse
                {
                    Id = massMessage.Id, 
                    HasRecipients = massMessage.Recipients.Any(),
                    Languages = languages
                };
            }
        }

        private void DeleteUnsentMassMessages()
        {
            var adminId = _actorInfoProvider.Actor.Id;

            var massMessage = _messagingRepository
                .MassMessages
                .Include(x => x.Recipients)
                .SingleOrDefault(x => x.AdminId == adminId && x.DateSent == null);

            if (massMessage == null) return;

            massMessage.Recipients.Clear();
            _messagingRepository.MassMessages.Remove(massMessage);
        }

        [Permission(Permissions.Send, Module = Modules.MassMessageTool)]
        public SendMassMessageResponse Send(SendMassMessageRequest request)
        {
            var validationResult = _massMessageQueries.GetValidationResult(request);

            if (!validationResult.IsValid)
                throw new RegoValidationException(validationResult);

            _eventBus.Publish(new MassMessageSendRequestedEvent(
                request,
                DateTimeOffset.UtcNow,
                _actorInfoProvider.Actor.UserName));

            return new SendMassMessageResponse {IsSent = true};
        }
    }
}