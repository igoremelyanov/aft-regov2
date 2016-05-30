﻿using System;

namespace AFT.RegoV2.Core.Common.Data.Payment
{
    public class Exemption
    {
        public Guid PlayerId { get; set; }
        public bool Exempt { get; set; }
        public string ExemptFrom { get; set; }
        public string ExemptTo { get; set; }
        public int ExemptLimit { get; set; }
    }
}
