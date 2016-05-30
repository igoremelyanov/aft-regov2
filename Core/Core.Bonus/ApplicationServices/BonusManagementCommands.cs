using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Bonus.Core.Data;
using AFT.RegoV2.Bonus.Core.DomainServices;
using AFT.RegoV2.Core.Auth.Interface;
using AFT.RegoV2.Bonus.Core.Models.Commands;
using AFT.RegoV2.Bonus.Core.Models.Data;
using AFT.RegoV2.Bonus.Core.Models.Enums;
using AFT.RegoV2.Bonus.Core.Models.Events.Management;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.RegoBus.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Bonus.Core.ApplicationServices
{
    public class BonusManagementCommands
    {
        private readonly IBonusRepository _repository;
        private readonly BonusQueries _bonusQueries;
        private readonly IActorInfoProvider _actorInfoProvider;
        private readonly IEventBus _eventBus;
        private readonly BonusMapper _bonusMapper;

        public BonusManagementCommands(
            IBonusRepository repository,
            BonusQueries bonusQueries,
            IActorInfoProvider actorInfoProvider,
            IEventBus eventBus,
            BonusMapper bonusMapper)
        {
            _repository = repository;
            _bonusQueries = bonusQueries;
            _actorInfoProvider = actorInfoProvider;
            _eventBus = eventBus;
            _bonusMapper = bonusMapper;
        }

        public Guid AddUpdateBonus(CreateUpdateBonus model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
                throw new RegoException(string.Join("/n", validationResult.Errors.Select(failure => failure.ErrorMessage)));

            var bonus = _bonusMapper.MapModelToBonus(model);

            return model.Id == Guid.Empty ? AddBonus(bonus) : UpdateBonus(bonus);
        }

        private Guid AddBonus(Data.Bonus bonus)
        {
            bonus.Id = Guid.NewGuid();
            bonus.Template = _repository.Templates
                .Include(o => o.Info)
                .Include(o => o.Info.Brand)
                .Single(a => a.Id == bonus.Template.Id
                    && a.Version == bonus.Template.Version);

            bonus.CreatedOn = DateTimeOffset.Now.ToBrandOffset(bonus.Template.Info.Brand.TimezoneId);
            bonus.CreatedBy = _actorInfoProvider.Actor.UserName;
            bonus.Statistic = new BonusStatistic();

            _repository.Bonuses.Add(bonus);
            _repository.SaveChanges();

            _eventBus.Publish(new BonusCreated
            {
                AggregateId = bonus.Id,
                Description = bonus.Description,
                Name = bonus.Name,
                Code = bonus.Code,
                BrandId = bonus.Template.Info.Brand.Id,
                BonusType = bonus.Template.Info.TemplateType,
                IsActive = bonus.IsActive,
                EventCreated = bonus.CreatedOn
            });

            return bonus.Id;
        }

        private Guid UpdateBonus(Data.Bonus updatedBonus)
        {
            AddUpdatedBonus(updatedBonus);
            _eventBus.Publish(new BonusUpdated
            {
                AggregateId = updatedBonus.Id,
                Description = updatedBonus.Description,
                EventCreated = updatedBonus.UpdatedOn.Value
            });

            return updatedBonus.Id;
        }

        private void AddUpdatedBonus(Data.Bonus updatedBonus)
        {
            updatedBonus.Template = _repository.Templates.Single(a => a.Id == updatedBonus.Template.Id && a.Version == updatedBonus.Template.Version);

            var firstBonusVersion = _repository.Bonuses
                .Where(bonus => bonus.Id == updatedBonus.Id)
                .OrderBy(b => b.CreatedOn)
                .First();
            updatedBonus.CreatedOn = firstBonusVersion.CreatedOn;
            updatedBonus.CreatedBy = firstBonusVersion.CreatedBy;
            updatedBonus.UpdatedOn = DateTimeOffset.Now.ToBrandOffset(updatedBonus.Template.Info.Brand.TimezoneId);
            updatedBonus.UpdatedBy = _actorInfoProvider.Actor.UserName;

            //to persist bonus statistic
            updatedBonus.Statistic = _repository.GetCurrentVersionBonuses().Single(a => a.Id == updatedBonus.Id).Statistic;
            updatedBonus.Version++;

            _repository.Bonuses.Add(updatedBonus);
            _repository.SaveChanges();
        }

        public void ChangeBonusStatus(ToggleBonusStatus model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
                throw new RegoException(validationResult.Errors.First().ErrorMessage);

            var bonusToUpdate = _bonusQueries.GetCurrentVersionBonuses().Single(b => b.Id == model.Id);
            var isActive = model.IsActive;
            if (bonusToUpdate.IsActive == isActive)
                return;

            bonusToUpdate.IsActive = isActive;
            AddUpdatedBonus(bonusToUpdate);

            if (isActive)
                _eventBus.Publish(new BonusActivated
                {
                    AggregateId = bonusToUpdate.Id,
                    Remarks = model.Remarks,
                    EventCreated = bonusToUpdate.UpdatedOn.Value
                });
            else
                _eventBus.Publish(new BonusDeactivated
                {
                    AggregateId = bonusToUpdate.Id,
                    Remarks = model.Remarks,
                    EventCreated = bonusToUpdate.UpdatedOn.Value
                });
        }

        public TemplateIdentifier AddUpdateTemplate(CreateUpdateTemplate model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
                throw new RegoException(string.Join("/n", validationResult.Errors.Select(failure => failure.ErrorMessage)));

            var template = _bonusMapper.MapModelToTemplate(model);

            template.Info.Brand = _repository.Brands.Single(brand => brand.Id == template.Info.Brand.Id);

            if (template.Id == Guid.Empty)
            {
                template.Id = Guid.NewGuid();
                template.CreatedOn = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId);
                template.CreatedBy = _actorInfoProvider.Actor.UserName;

                _repository.Templates.Add(template);
                _repository.SaveChanges();
            }
            else
            {
                var firstTemplateVersion = _repository.Templates.Where(t => t.Id == template.Id).Single(t => t.Version == 0);
                template.CreatedOn = firstTemplateVersion.CreatedOn;
                template.CreatedBy = firstTemplateVersion.CreatedBy;
                template.UpdatedOn = DateTimeOffset.Now.ToBrandOffset(template.Info.Brand.TimezoneId);
                template.UpdatedBy = _actorInfoProvider.Actor.UserName;
                if (template.Status == TemplateStatus.Complete)
                {
                    var bonusesUsingThisTemplate = _bonusQueries.GetBonusesUsingTemplate(template).ToList();

                    template.Version++;
                    _eventBus.Publish(new BonusTemplateUpdated
                    {
                        AggregateId = template.Id,
                        Description = template.Info.Description,
                        EventCreated = template.UpdatedOn.Value
                    });

                    _repository.Templates.Add(template);
                    _repository.SaveChanges();

                    foreach (var bonus in bonusesUsingThisTemplate)
                    {
                        bonus.Template = template;
                        UpdateBonus(bonus);
                    }
                }
                else
                {
                    if (template.Notification != null)
                    {
                        template.Status = TemplateStatus.Complete;
                        _eventBus.Publish(new BonusTemplateCreated
                        {
                            AggregateId = template.Id,
                            Description = template.Info.Description,
                            EventCreated = template.CreatedOn
                        });
                    }
                    _repository.Templates.Remove(firstTemplateVersion);

                    _repository.Templates.Add(template);
                    _repository.SaveChanges();
                }
            }

            return new TemplateIdentifier
            {
                Id = template.Id,
                Version = template.Version
            };
        }

        public void DeleteTemplate(DeleteTemplate model)
        {
            var validationResult = _bonusQueries.GetValidationResult(model);
            if (validationResult.IsValid == false)
                throw new RegoException(validationResult.Errors.First().ErrorMessage);

            var templates = _repository.Templates.Where(t => t.Id == model.TemplateId);
            templates.ToList().ForEach(template => template.Status = TemplateStatus.Deleted);
            var lastVersion = templates.Max(t => t.Version);
            var lastTemplate = templates.Single(t => t.Version == lastVersion);
            lastTemplate.UpdatedOn = DateTimeOffset.Now.ToBrandOffset(lastTemplate.Info.Brand.TimezoneId);
            lastTemplate.UpdatedBy = _actorInfoProvider.Actor.UserName;

            _repository.SaveChanges();
        }
    }
}