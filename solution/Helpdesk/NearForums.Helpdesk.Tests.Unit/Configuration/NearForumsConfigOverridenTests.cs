using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.Base.Principal;
using NearForums.Helpdesk.Configuration;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Tests.Unit.Configuration
{
    public class NearForumsConfigOverridenTests
    {
        [Test]
        public void ShouldFailIfIdentityIsNotIHelpdeskIdentity()
        {
            NearForumConfigOverriden config = GetConfig();
            config.Search.Context = GetContext();

            Assert.Throws<ConfigurationErrorsException>(
                () => { string indexPath = config.Search.IndexPath; });
        }

        [Test]
        public void ShouldFailIfUserNotLoggedIn()
        {
            NearForumConfigOverriden config = GetConfig();
            config.Search.Context = GetContext(true);

            Assert.Throws<ConfigurationErrorsException>(
                () => { string indexPath = config.Search.IndexPath; });
        }

        [Test]
        public void ShouldIncludeHelpdeskNameInIndexPath()
        {
            NearForumConfigOverriden config = GetConfig();
            config.Search.Context = GetContext(true, IdentityData.TEST_HELPDESK_UNIQUE_NAME);

            Assert.IsTrue(config.Search.IndexPath.EndsWith(IdentityData.TEST_HELPDESK_UNIQUE_NAME));
        }

        private NearForumConfigOverriden GetConfig()
        {
            NearForumConfigOverriden config = (NearForumConfigOverriden)ConfigurationManager.GetSection("site");
            
            config.Search.Context = CreateContextForAuthenticatedUser();
            
            return config;
        }

        private IHttpContext GetContext(bool containsIHelpdeskIdentity = false, string helpdeskName = null)
        {
            IHttpContext context = Substitute.For<IHttpContext>();

            if (containsIHelpdeskIdentity || !string.IsNullOrEmpty(helpdeskName))
            {
                IHelpdeskIdentity identity = Substitute.For<IHelpdeskIdentity>();
                context.User.Identity.Returns(identity);
            }

            if (!string.IsNullOrEmpty(helpdeskName))
            {
                IHelpdeskIdentity identyty = (IHelpdeskIdentity)context.User.Identity;
                identyty.HelpdeskUniqueName.Returns(helpdeskName);
                identyty.IsAuthenticated.Returns(true);
            }

            return context;
        }

        private IHttpContext CreateContextForAuthenticatedUser()
        {
            IHttpContext context = Fakes.WebFakesFactory.Create<IHttpContext>();
            context.User = Substitute.For<IPrincipal>();

            IHelpdeskIdentity identity = Substitute.For<IHelpdeskIdentity>();
            identity.IsAuthenticated.Returns(true);

            return context;
        }
    }
}
