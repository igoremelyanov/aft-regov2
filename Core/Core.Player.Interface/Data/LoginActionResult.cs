using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Player.Interface.Data
{
    public class PlayerCommandResult
    {
        public bool Success { get; set; }
        public Common.Data.Player.Player Player { get; set; }
        public ValidationResult ValidationResult { get; set; }
    }

    public class LoginRequestContext
    {
        public Dictionary<string,string> BrowserHeaders { get; set; }
        public string IpAddress { get; set; }
        public Guid BrandId { get; set; }
    }
}
