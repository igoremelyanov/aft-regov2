using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AFT.RegoV2.Core.Auth.Data;

namespace AFT.RegoV2.Core.Auth.Entities
{
    public class Actor
    {
        internal Data.Actor Data { get; set; }

        public Actor(Guid id, string username, string password)
        {
            Data = new Data.Actor
            {
                Id = id,
                Username = username,
                EncryptedPassword = EncryptPassword(id, password)
            };
        }

        public Actor(Data.Actor data)
        {
            Data = data;
        }

        public void ChangePassword(string password)
        {
            Data.EncryptedPassword = EncryptPassword(Data.Id, password);
        }

        string EncryptPassword(Guid userId, string password)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.Unicode.GetBytes(userId.ToString().Substring(0, 4) + password)));
        }

        public bool HasPermissionForModule(string permissionName, string module)
        {
            if (Data.Role == null)
                return false;

            return Data.Role.Permissions.Any(p => p.Name == permissionName && p.Module == module);
        }

        public IEnumerable<Permission> GetPermissions()
        {
            if (Data.Role == null)
                return new List<Permission>();

            return Data.Role.Permissions;
        }

        public void AssignRole(Role role)
        {
            Data.Role = role;
        }
    }
}
