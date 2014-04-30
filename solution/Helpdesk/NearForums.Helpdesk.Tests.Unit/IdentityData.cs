using NearForums.Helpdesk.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Tests.Unit
{
    internal class IdentityData
    {
        public static class Cookie
        {
            public const string VALID_AUTH_STRING = "OGU0ZDBmNDhiMDMxMjg3NTJlNzVlMWYyMmNjYmVkOWM7MjAxNC0wNC0yNFQwNzo0NTo1MS42MzQxO0ZhbHNlO0JELTA3LUMzLTQ5LUU3LTYzLUE1LTM3LUEwLTk0LUQ1LTNELUM5LTQzLUZFLTlFLURGLTE3LUZCLTcxLUExLTc5LTU0LTIwLTg4LTFELUZCLTIzLTA2LURELTM0LUNFLUVGLTA4LTU1LTM4LTA5LTEyLTVELTMwLTQzLTRFLTJFLTQ1LTExLUJBLTRGLTdELUM3LUNELTFDLTZGLTk0LTY5LTcxLUNFLTkyLTNDLTRGLUJBLTlBLTE2LTAwLTEw";
            public const string VALID_COOKIES_IDCS = "8e4d0f48b03128752e75e1f22ccbed9c";
            public const bool VALID_COOKIES_REMEMBER_ME = false;
        }

        public static class SecretToken
        {
            public const string SECRET_TOKEN_1 = "ad37e5ab-ac25-44ca-8cd5-57a5a1dee9f6";
            public const string SECRET_TOKEN_2 = "3c6b5429-08fc-4937-9f8a-ee481cfc91fc";
        }

        public static string[] SkipExtensions = new string[] { ".jpg", ".png" };
        
        public const string TEST_HELPDESK_UNIQUE_NAME = "testHelpdesk";

        public const string TEST_PROVIDER_NAME = "testProvider";

        public static HDMembershipUser HDMembershipuserForValidCookie = CreateHDMembershipUser();

        public static DbUser DbUserForValidCookie = new DbUser()
        {
            Idcs = IdentityData.Cookie.VALID_COOKIES_IDCS,
            Email = "user@server.com",
            HelpdeskUniqueName = IdentityData.TEST_HELPDESK_UNIQUE_NAME,
            Name = "Test User",
            PasswordHash = "asdf",
            PasswordSalt = "asd"
        };

        private static HDMembershipUser CreateHDMembershipUser()
        {
            System.Web.Security.MembershipProvider provider = new HelpdeskMembershipProvider();
            provider.Initialize(TEST_PROVIDER_NAME, new System.Collections.Specialized.NameValueCollection());

            //System.Web.Security.Membership.Providers
            //    .GetType()
            typeof(System.Configuration.Provider.ProviderCollection)
                .GetField("_ReadOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(
                    System.Web.Security.Membership.Providers,
                    false);

            System.Web.Security.Membership.Providers.Add(provider);

            HDMembershipUser user = new HDMembershipUser(
                TEST_PROVIDER_NAME,
                "Test user",
                "testProviderKey",
                "user@server.com",
                null,
                null,
                true,
                false,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MinValue,
                TEST_HELPDESK_UNIQUE_NAME);

            return user;
        }


    }
}
