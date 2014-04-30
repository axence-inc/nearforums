using NearForums.Helpdesk.DataAccess.Data;
using NearForums.Helpdesk.Tests.Unit.Fakes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;

namespace NearForums.Helpdesk.Tests.Unit.DataAccess
{
    public class NFMaintenanceDataAccessTests : DataAccessTestsBase
    {
        private class NFMaintenanceDataAccessForTests : NFMaintenanceDataAccess
        {
            public NFMaintenanceDataAccessForTests()
                : base(GetSettings(), IdentityData.TEST_HELPDESK_UNIQUE_NAME)
            {
                base.dbProviderFactoryDefault = DataAccessFakeFactory.Create<DbProviderFactory>();
            }
        }

        [Test]
        public void ShuldCreateSchemaIfNotExists()
        {
            NFMaintenanceDataAccessForTests testObject = new NFMaintenanceDataAccessForTests();
            DataAccessFakeFactory.executeNonQuery.Add(new Func<int>(() => { throw new Exception(); }));
            
            testObject.EnsureProperSchemaExists();

            Assert.AreEqual(2, DataAccessFakeFactory.commands.Count());
            
            
            Assert.AreEqual(CHECK_SCHEMA_EXIST_QUERY, DataAccessFakeFactory.commands[0].CommandText);
            Assert.AreEqual(0, DataAccessFakeFactory.parameters.Count());
            DataAccessFakeFactory.commands[0].Received(1).ExecuteNonQuery();

            Assert.IsTrue(512 < DataAccessFakeFactory.commands[1].CommandText.Length);
            DataAccessFakeFactory.commands[1].Received(1).ExecuteNonQuery();
        }

        public void ShouldNotTryToCreateSchemaIfExists()
        {
            NFMaintenanceDataAccessForTests testObject = new NFMaintenanceDataAccessForTests();

            testObject.EnsureProperSchemaExists();

            Assert.AreEqual(1, DataAccessFakeFactory.commands.Count());
            Assert.AreEqual(CHECK_SCHEMA_EXIST_QUERY, DataAccessFakeFactory.commands[0].CommandText);
            Assert.AreEqual(0, DataAccessFakeFactory.parameters.Count());
        }

        string CHECK_SCHEMA_EXIST_QUERY = "SELECT * FROM ForumsCategories LIMIT 1";
    }
}
