using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.DataAccess.Model
{
    internal class ClientSideSession
    {
        public static ClientSideSession Get(string base64cookie, string secretToken1, string secretToken2)
        {
            ClientSideSession session = null;

            try
            {
                session = new ClientSideSession(base64cookie);
            }
            catch (FormatException exc)
            {
                throw new InvalidClientSideSession("Invalid auth string", exc);
            }

            string generatedChecksum = CreateSha512(
                session.UserIdcs,
                secretToken1,
                session.CreateDate.ToString("yyyy-MM-ddTHH:mm:ss.ffff"),
                session.UserIdcs,
                secretToken2,
                session.RememberMe.ToString());

            if (!generatedChecksum.Equals(session.Checksum))
            {
                throw new InvalidClientSideSession("Invalid auth string");
            }

            return session;
        }

        private ClientSideSession(string base64cookie)
        {
            string[] tokens = FromBase64(base64cookie).Split(';');
            if (tokens.Length != 4)
            {
                throw new InvalidClientSideSession("Invalid number of tokens");
            }

            this.UserIdcs = tokens[0];
            this.CreateDate = DateTime.Parse(tokens[1]);
            this.Remember = bool.Parse(tokens[2]);
            this.Checksum = tokens[3];
        }

        public string UserIdcs { get; set; }

        public bool RememberMe { get; set; }

        protected DateTime CreateDate { get; set; }

        protected string Checksum { get; set; }

        protected bool Remember { get; set; }

        protected string FromBase64(string source)
        {
            string decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(source));

            return decoded;
        }

        private static string CreateSha512(params string[] values)
        {
            var sha512Provider = new SHA512CryptoServiceProvider();
            var utf8 = new UTF8Encoding();

            return BitConverter
                .ToString(sha512Provider.ComputeHash(utf8.GetBytes(string.Join(string.Empty, values))));
        }

        public string helpdeskUniqueName { get; set; }

        public class InvalidClientSideSession : FormatException
        {
            public InvalidClientSideSession(string msg)
                : this(msg, null)
            {
            }

            public InvalidClientSideSession(string msg, Exception exc)
                :base(msg, exc)
            {
            }
        }
    }
}
