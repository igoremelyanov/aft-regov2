using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AFT.RegoV2.Core.Common
{
    public interface IFileStorage
    {
        Guid Save(string fileName, byte[] content);
        byte[] Get(Guid fileId);
    }

    public class FileSystemStorage : IFileStorage
    {
        private object thisLock = new object();
        private readonly string ConnStr = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        public Guid Save(string fileName, byte[] content)
        {
            Guid insertedId;

            fileName = Guid.NewGuid() + "_" + fileName;

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (var txn = conn.BeginTransaction())
                {
                    using (var cmd = new SqlCommand("INSERT INTO doc.Documents(file_stream, name) OUTPUT INSERTED.stream_id VALUES (@file_stream, @name)", conn, txn))
                    {
                        cmd.Parameters.Add("@file_stream", SqlDbType.VarBinary).Value = content;
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = fileName;
                        insertedId = (Guid)cmd.ExecuteScalar();
                    }

                    txn.Commit();
                }

                conn.Close();
            }

            return insertedId;
        }

        public byte[] Get(Guid fileId)
        {
            byte[] result = { };

            lock (thisLock)
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    var cmd =
                        new SqlCommand("SELECT file_stream FROM doc.Documents WITH (READCOMMITTEDLOCK) WHERE stream_id = @id",
                            conn);
                    cmd.Parameters.Add("@id", SqlDbType.VarChar).Value = fileId.ToString();

                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                        result = (byte[])reader["file_stream"];
                }
            }

            return result;
        }
    }
}
