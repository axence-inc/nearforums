using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.DataAccess.Data;
using NearForums.Helpdesk.DataAccess.Model;
using NearForums.Helpdesk.Modules;
using NearForums.Helpdesk.Tests.Unit.Fakes;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Tests.Unit.Modules
{
    public class DbMaintenanceTests
    {
        [Test]
        public void MaintenanceShouldFailIfNoAuthenticatedUsers()
        {
            DbMaintenanceForTests testModule = new DbMaintenanceForTests();
            Assert.Throws<System.Security.Authentication.AuthenticationException>(
                () => testModule.CallPostAuthenticateRequestHandle());

            testModule.GetNFMaintenanceDataAccess().DidNotReceiveWithAnyArgs().EnsureProperSchemaExists();
        }

        [Test]
        public void MaintenanceShouldSucceedIfUserAuthenticated()
        {
            DbMaintenanceForTests testModule = new DbMaintenanceForTests();
            IPrincipal principal = Fakes.WebFakesFactory.Create<IPrincipal>();
            testModule.HttpContext.User.Returns(principal);
            testModule.HttpContext.User.Identity.IsAuthenticated.Returns(true);

            testModule.CallPostAuthenticateRequestHandle();
            testModule.GetNFMaintenanceDataAccess().Received(1).EnsureProperSchemaExists();
        }

        private class DbMaintenanceForTests : DbMaintenance
        {
            public DbMaintenanceForTests()
            {
                base.MaintenanceDataAccess = Substitute.For<INFMaintenanceDataAccess>();
                base.HttpContext = WebFakesFactory.Create<IHttpContext>();
                base.Config = WebFakesFactory.Create<IHelpdeskConfig>();
            }

            public INFMaintenanceDataAccess GetNFMaintenanceDataAccess()
            {
                return base.MaintenanceDataAccess;
            }

            public void CallPostAuthenticateRequestHandle()
            {
                base.context_PostAuthenticateRequest(null, null);
            }
        }
    }
}
