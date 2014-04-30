using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.Services;
using NearForums.Helpdesk.Tests.Unit.Fakes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;

namespace NearForums.Helpdesk.Tests.Unit.Services
{
    public class AuthorizationServiceTests
    {
        [Test]
        public void ShouldThrowExceptionOnNullValue()
        {
            AuthorizationServiceForTests service = new AuthorizationServiceForTests();

            Assert.Throws<System.Security.Authentication.AuthenticationException>(
                () => service.Authorize(null));
        }

        [Test]
        public void ShouldThrowExceptionOnCookiesInvalidChecksum()
        {
            StringBuilder invalidCookieBuilder = new StringBuilder(IdentityData.Cookie.VALID_AUTH_STRING);
            invalidCookieBuilder[5] = (invalidCookieBuilder[5] == 'Z' ? 'X' : 'Z');

            AuthorizationServiceForTests service = new AuthorizationServiceForTests();

            Assert.Throws<System.Security.Authentication.AuthenticationException>(
                () => service.Authorize(invalidCookieBuilder.ToString()));
        }

        [Test]
        public void ShouldAuthenticateOnValidCredentials()
        {
            AuthorizationServiceForTests service = new AuthorizationServiceForTests();
            service.MembershipProvider.GetUser((object)IdentityData.Cookie.VALID_COOKIES_IDCS, true).Returns(IdentityData.HDMembershipuserForValidCookie);

            service.Authorize(IdentityData.Cookie.VALID_AUTH_STRING);

            Assert.IsTrue(service.HttpContext.User.Identity.IsAuthenticated);
            Assert.AreEqual(service.HttpContext.User.Identity, IdentityData.HDMembershipuserForValidCookie);
        }

        private class AuthorizationServiceForTests : AuthorizationService
        {
            public AuthorizationServiceForTests()
            {
                base.Config = WebFakesFactory.Create<IHelpdeskConfig>();
                base.HttpContext = WebFakesFactory.Create<IHttpContext>();
                base.MembershipProvider = WebFakesFactory.Create<System.Web.Security.MembershipProvider>();
            }
        }
    }
}
