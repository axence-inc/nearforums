using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.DataAccess.Model;
using NearForums.Helpdesk.DataProvider;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NearForums.Helpdesk.Base.Principal;

namespace NearForums.Helpdesk.Tests.Unit.DataProvider
{
    public class FactoryTests
    {
        [Test]
        public void ShouldCreateCorrectCommand()
        {
            FactoryForTests factory = new FactoryForTests();
            ((IHelpdeskIdentity)factory.GetHttpContext().User.Identity).HelpdeskUniqueName = IdentityData.TEST_HELPDESK_UNIQUE_NAME;
            ((IHelpdeskIdentity)factory.GetHttpContext().User.Identity).IsAuthenticated.Returns(true);
            
            DbCommand dbCmd = factory.CreateCommand();

            Assert.IsInstanceOf<Helpdesk.DataProvider.Command>(dbCmd);
            Command cmd = (Command)dbCmd;

            Assert.AreEqual(IdentityData.TEST_HELPDESK_UNIQUE_NAME, cmd.Schema);
        }

        [Test]
        public void ShouldFailIfUserNotAuthenticated()
        {
            FactoryForTests factory = new FactoryForTests();
            Assert.Throws<Factory.AccessDeniedException>(() => factory.CreateCommand());
        }

        [Test]
        public void ShouldCreateCorrectConnection()
        {
            FactoryForTests factory = new FactoryForTests();

            Assert.IsInstanceOf<Npgsql.NpgsqlConnection>(factory.CreateConnection());
        }

        [Test]
        public void ShouldCreateCorrectDataAdapter()
        {
            FactoryForTests factory = new FactoryForTests();

            Assert.IsInstanceOf<Npgsql.NpgsqlDataAdapter>(factory.CreateDataAdapter());
        }

        private class FactoryForTests : Helpdesk.DataProvider.Factory
        {
            public FactoryForTests()
            {
                base.httpContext = Fakes.WebFakesFactory.Create<IHttpContext>();
                base.httpContext.User = Fakes.WebFakesFactory.Create<IPrincipal>();
            }

            public IHttpContext GetHttpContext()
            {
                return base.httpContext;
            }
        }
    }
}
