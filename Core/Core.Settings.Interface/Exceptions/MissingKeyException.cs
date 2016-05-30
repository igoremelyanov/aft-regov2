using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Settings.Interface.Exceptions
{
    public class MissingKeyException: Exception
    {
        public MissingKeyException() : base("Key is not present") {}
        public MissingKeyException(string key) : base(string.Format("Key '{0}' is not present", key)) {}
    }
}