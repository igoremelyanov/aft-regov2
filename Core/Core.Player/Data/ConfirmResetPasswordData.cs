﻿using System;

namespace AFT.RegoV2.Core.Player.Data
{
    public class ConfirmResetPasswordData
    {
        public Guid PlayerId { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
