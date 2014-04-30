using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace NearForums.Helpdesk.DataAccess.Model
{
    [DataContract(Name = "endusers")]
    public class DbUser
    {
        [DataMember(Name = "signature")]
        public string Name { get; set; }

        [DataMember(Name = "password_hash")]
        public string PasswordHash { get; set; }

        [DataMember(Name = "password_salt")]
        public string PasswordSalt { get; set; }

        [DataMember(Name = "idcs")]
        public object Idcs { get; set; }

        [DataMember(Name = "primary_email")]
        public string Email { get; set; }

        [DataMember(Name = "helpdesk_unique_name")]
        public string HelpdeskUniqueName { get; set; }

        public bool ValidatePass(string password)
        {
            string generatedHash = this.GenerateHash(this.PasswordSalt, password);
            bool hashesMatch = !string.IsNullOrEmpty(this.PasswordHash) && this.PasswordHash.Equals(generatedHash);

            return hashesMatch;
        }

        private string GenerateHash(string salt, string password)
        {
            var sha512Provider = new SHA512CryptoServiceProvider();

            UTF8Encoding utf8 = new UTF8Encoding();

            string hash = BitConverter.ToString(sha512Provider.ComputeHash(utf8.GetBytes(salt + password))).Replace("-", "");

            return hash;
        }
    }
}
