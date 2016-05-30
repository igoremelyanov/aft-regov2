using System;
using AFT.RegoV2.Core.Auth.Interface.ApplicationServices;
using AFT.RegoV2.Core.Auth.Interface.Data;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using FluentValidation;
using FluentValidation.Results;
using ServiceStack;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class LoginValidator : AbstractValidator<Common.Data.Player.Player>
    {
        public LoginValidator(BrandQueries brandQueries, IAuthQueries authQueries, string password)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(player => player)
                .NotNull()
                .WithMessage(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString())
                .WithName("Player");

            When(player => player != null, () =>
            {
                RuleFor(player => player.IsInactive)
                    .NotEqual(true)
                    .WithMessage(PlayerAccountResponseCode.NonActive.ToString());

                RuleFor(player => player.IsLocked)
                    .NotEqual(true)
                    .WithMessage(PlayerAccountResponseCode.AccountLocked.ToString());

                RuleFor(player => player.BrandId)
                    .Must(brandQueries.IsBrandActive)
                    .WithMessage(PlayerAccountResponseCode.InactiveBrand.ToString());

                if (password != null)
                    RuleFor(player => player)
                        .Must(player =>
                        {
                            var loginValidationResult = authQueries.GetValidationResult(new LoginActor
                            {
                                ActorId = player.Id,
                                Password = password
                            });

                            return loginValidationResult.IsValid;
                        })
                        .WithMessage(PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString())
                        .WithName("Player");

                RuleFor(player => player.SelfExclusion)
                    .Must(selfExclusion => !selfExclusion.HasValue || selfExclusion.Value != SelfExclusion.Permanent)
                    .WithMessage(PlayerAccountResponseCode.SelfExcludedPermanent.ToString(),
                        player => new ValidationParam
                        {
                            Name = "start",
                            Value = player.SelfExclusionDate.Value.ToString("yyyy/MM/dd hh:mm tt")
                        })
                    .Must(selfExclusion => !selfExclusion.HasValue)
                    .WithMessage(PlayerAccountResponseCode.SelfExcluded.ToString(),
                        player => new ValidationParam
                        {
                            Name = "start",
                            Value = player.SelfExclusionDate.Value.ToString("yyyy/MM/dd hh:mm tt")
                        },
                        player =>
                        {
                            var endDate = ExclusionDateHelper.GetSelfExcusionEndDate(player.SelfExclusion.Value,
                                player.SelfExclusionDate.Value);

                            return new ValidationParam
                            {
                                Name = "end",
                                Value = endDate.ToString("yyyy/MM/dd hh:mm tt")
                            };
                        },
                        player =>
                        {
                            return new ValidationParam
                            {
                                Name = "length",
                                Value = Enum.GetName(typeof(SelfExclusion), player.SelfExclusion.Value)
                            };
                        });

                RuleFor(player => player.TimeOut.HasValue)
                    .NotEqual(true)
                    .WithMessage(PlayerAccountResponseCode.TimedOut.ToString(),
                        player => new ValidationParam
                        {
                            Name = "start",
                            Value = player.TimeOutDate.Value.ToString("yyyy/MM/dd hh:mm tt")
                        },
                        player =>
                        {
                            var endDate = ExclusionDateHelper.GetTimeOutEndDate(player.TimeOut.Value,
                                player.TimeOutDate.Value);

                            return new ValidationParam
                            {
                                Name = "end",
                                Value = endDate.ToString("yyyy/MM/dd hh:mm tt")
                            };
                        },
                        player =>
                        {
                            return new ValidationParam
                            {
                                Name = "length",
                                Value = Enum.GetName(typeof (TimeOut), player.TimeOut.Value)
                            };
                        });
            });
        }
    }

    public class ValidationParam
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}