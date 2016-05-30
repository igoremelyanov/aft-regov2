using System;

namespace AFT.RegoV2.Core.Messaging.Interface.Data.MassMessageCommands
{
    public class NewMassMessageDataResponse
    {
        public NewMassMessageLicensee[] Licensees { get; set; }
    }

    public class NewMassMessageLicensee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public NewMassMessageBrand[] Brands { get; set; }
    }

    public class NewMassMessageBrand
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public NewMassMessagePaymentLevel[] PaymentLevels { get; set; }
        public NewMassMessageVipLevel[] VipLevels { get; set; }
    }

    public class NewMassMessagePaymentLevel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class NewMassMessageVipLevel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}