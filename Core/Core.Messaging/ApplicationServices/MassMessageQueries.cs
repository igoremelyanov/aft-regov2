using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Messaging.Interface.ApplicationServices;
using AFT.RegoV2.Core.Messaging.Interface.Data;
using AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands;
using AFT.RegoV2.Core.Messaging.Validators;
using AutoMapper;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Messaging.ApplicationServices
{
    public class MassMessageQueries : MarshalByRefObject, IMassMessageQueries
    {
        private readonly IMessagingRepository _messagingRepository;

        public MassMessageQueries(
            IMessagingRepository messagingRepository)
        {
            _messagingRepository = messagingRepository;
        }

        static MassMessageQueries()
        {
            Mapper.CreateMap<Data.Player, Player>()
                .ForMember(dest => dest.BrandName, opts => opts.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.PaymentLevelName, opts => opts.MapFrom(src => src.PaymentLevel.Name))
                .ForMember(dest => dest.VipLevelName, opts => opts.MapFrom(src => src.VipLevel.Name));
        }

        public IQueryable<Player> CreateMassMessagePlayerQuery(SearchPlayersRequest request)
        {
            var query = _messagingRepository.Players
                .Include(x => x.PaymentLevel)
                .Include(x => x.VipLevel)
                .Where(x => x.BrandId == request.BrandId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();

                query = query.Where(x =>
                    x.Username.Contains(searchTerm) ||
                    x.FirstName.Contains(searchTerm) ||
                    x.LastName.Contains(searchTerm) ||
                    x.Email.Contains(searchTerm) ||
                    x.PhoneNumber.Contains(searchTerm));
            }

            if (request.PaymentLevelId.HasValue)
                query = query.Where(x => x.PaymentLevelId == request.PaymentLevelId);

            if (request.VipLevelId.HasValue)
                query = query.Where(x => x.VipLevelId == request.VipLevelId);

            if (request.PlayerStatus.HasValue)
            {
                var isActive = request.PlayerStatus == Status.Active;
                query = query.Where(x => x.IsActive == isActive);
            }

            if (request.RegistrationDateFrom.HasValue)
                query = query.Where(x => x.DateRegistered >= request.RegistrationDateFrom);

            if (request.RegistrationDateTo.HasValue)
            {
                var date = request.RegistrationDateTo.Value.AddDays(1);
                query = query.Where(x => x.DateRegistered < date);
            }

            return Mapper.Map<List<Player>>(query).AsQueryable();
        }

        public IQueryable<Player> GetRecipients(Guid massMessageId)
        {
            var recipients = _messagingRepository.MassMessages
                .Include(x => x.Recipients.Select(y => y.VipLevel))
                .Include(x => x.Recipients.Select(y => y.PaymentLevel))
                .Single(x => x.Id == massMessageId)
                .Recipients;

            return Mapper.Map<List<Player>>(recipients).AsQueryable();
        }

        public ValidationResult GetValidationResult(UpdateRecipientsRequest request)
        {
            return new UpdateMassMessageRecipientsValidator(_messagingRepository).Validate(request);
        }

        public ValidationResult GetValidationResult(SendMassMessageRequest request)
        {
            return new SendMassMessageValidator(_messagingRepository).Validate(request);
        }
    }
}