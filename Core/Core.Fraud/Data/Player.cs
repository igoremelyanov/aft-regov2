using System;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Core.Fraud.Data
{
    public class Player
    {
        public Guid Id { get; set; }
        public Guid BrandId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string IPAddress { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public SystemAction FolderAction { get; set; }
        public QueueFolderTag Tag { get; set; }
        public string FraudType { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public string SignUpRemark { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
        public DateTimeOffset? DuplicateCheckDate { get; set; }
        public DateTimeOffset? HandledDate { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
    }
}