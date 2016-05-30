using System.Data.SqlClient;

namespace AFT.UGS.Builds.UgsDatabaseHelper
{
    public class SecondaryReplicaInfo
    {
        public string Host { get; set; }
        public SqlConnection Connection { get; set; }
    }
}
