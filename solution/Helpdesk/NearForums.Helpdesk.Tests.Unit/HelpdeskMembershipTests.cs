using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk.DataAccess.Data;
using NearForums.Helpdesk.Tests.Unit.Fakes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.DataAccess.Model;
using System.Web.Security;

namespace NearForums.Helpdesk.Tests.Unit
{
    public class HelpdeskMembershipTests
    {
        [Test]
        public void ShouldReturnProperUser()
        {
            HelpdeskMembershipProviderForTests provider = new HelpdeskMembershipProviderForTests();
            provider.Initialize(IdentityData.TEST_PROVIDER_NAME, null);
            provider.DataAccess.GetUserById(IdentityData.Cookie.VALID_COOKIES_IDCS).Returns(IdentityData.DbUserForValidCookie);

            // this ensures test MembershipProvider is being added to the collection
            //var user = IdentityData.HDMembershipuserForValidCookie;

            MembershipUser user = provider.GetUser((object)IdentityData.Cookie.VALID_COOKIES_IDCS, true);
            
            Assert.IsNotNull(user);
            Assert.AreEqual(provider.Name, user.ProviderName);

            Assert.AreEqual(IdentityData.DbUserForValidCookie.Name, user.UserName);
            Assert.AreEqual(IdentityData.DbUserForValidCookie.Idcs, user.ProviderUserKey);
            Assert.AreEqual(IdentityData.DbUserForValidCookie.Email, user.Email);

            Assert.IsInstanceOf<HDMembershipUser>(user);
            HDMembershipUser hdUser = (HDMembershipUser)user;
            Assert.AreEqual(IdentityData.DbUserForValidCookie.HelpdeskUniqueName, hdUser.HelpdeskUniqueName);
        }

        private class HelpdeskMembershipProviderForTests : HelpdeskMembershipProvider
        {
            public HelpdeskMembershipProviderForTests()
            {
                base.Config = WebFakesFactory.Create<IHelpdeskConfig>();
                base.UserDataAccess = WebFakesFactory.Create<IHDUserDataAccess>();
                base.HttpContext = WebFakesFactory.Create<IHttpContext>();
            }

            public IHDUserDataAccess DataAccess { get { return base.UserDataAccess; } }

        }
    }
}
