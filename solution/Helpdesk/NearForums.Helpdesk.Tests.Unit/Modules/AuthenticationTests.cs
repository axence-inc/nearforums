using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.Modules;
using NearForums.Helpdesk.Services;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Tests.Unit.Modules
{
    [TestFixture]
    public class AuthenticationTests
    {
        [Test]
        public void ShouldSkipsStaticPages()
        {
            AuthenticationModuleForTests authModule = new AuthenticationModuleForTests();
            authModule.HttpContext.Request.CurrentExecutionFilePathExtension.Returns(IdentityData.SkipExtensions[0]);

            authModule.CallAuthenticateRequestHandlerWithNullArgs();
            authModule.GetAuthenticationService().DidNotReceiveWithAnyArgs().Authorize(null);
        }

        [Test]
        public void ShouldReceiveExceptionOnMissingCookie()
        {
            AuthenticationModuleForTests authModule = new AuthenticationModuleForTests();
            Assert.Throws<System.Security.Authentication.AuthenticationException>(
                () => authModule.CallAuthenticateRequestHandlerWithNullArgs());
            authModule.GetAuthenticationService().DidNotReceiveWithAnyArgs().Authorize(null);
        }

        [Test]
        public void ShouldAuthenticate()
        {
            AuthenticationModuleForTests authModule = new AuthenticationModuleForTests();
            authModule.HttpContext.Request.Headers["Cookie"] =
                string.Format("{0}={1};",
                authModule.Config.CookieName,
                IdentityData.Cookie.VALID_AUTH_STRING);

            authModule.CallAuthenticateRequestHandlerWithNullArgs();
            authModule.GetAuthenticationService().Received(1).Authorize(IdentityData.Cookie.VALID_AUTH_STRING);
        }

        private class AuthenticationModuleForTests : Authentication
        {
            public AuthenticationModuleForTests()
            {
                base.AuthenticationService = Substitute.For<IAuthorizationService>();
                base.Config = Fakes.WebFakesFactory.Create<IHelpdeskConfig>();
                base.HttpContext = Fakes.WebFakesFactory.Create<IHttpContext>();

                NameValueCollection headers = new NameValueCollection() { 
                    { "Cookie", "someKey=aValue;" } 
                };

                base.HttpContext.Request.Headers.Returns(headers);
            }

            public void CallAuthenticateRequestHandlerWithNullArgs()
            {
                base.context_AuthenticateRequest(null, null);
            }

            public IAuthorizationService GetAuthenticationService()
            {
                return base.AuthenticationService;
            }
        }
    }
}
