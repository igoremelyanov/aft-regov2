using System;

namespace AFT.RegoV2.Core.Common.Data.Security.Users
{
    public abstract class ChangeUserStatusData
    {
        public Guid Id { get; set; }
        public string Remarks { get; set; }

        protected ChangeUserStatusData() { }

        protected ChangeUserStatusData(Guid id, string remarks)
        {
            Id = id;
            Remarks = remarks;
        }
    }

    public class ActivateUserData : ChangeUserStatusData
    {
        public ActivateUserData() { }

        public ActivateUserData(Guid id, string remarks) : base(id, remarks) { }
    }

    public class DeactivateUserData : ChangeUserStatusData
    {
        public DeactivateUserData() { }

        public DeactivateUserData(Guid id, string remarks) : base(id, remarks) { }
    }
}
