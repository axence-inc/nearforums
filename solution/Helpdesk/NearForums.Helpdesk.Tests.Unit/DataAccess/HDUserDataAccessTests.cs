using NearForums.Helpdesk.DataAccess.Data;
using NearForums.Helpdesk.DataAccess.Model;
using NearForums.Helpdesk.Tests.Unit.Fakes;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Tests.Unit.DataAccess
{
    public class HDUserDataAccessTests : DataAccessTestsBase
    {
        private class HDUserDataAccessForTests : HDUserDataAccess
        {
            public HDUserDataAccessForTests()
                : base(GetSettings())
            {
                base.dbProviderFactoryDefault = Fakes.DataAccessFakeFactory.Create<DbProviderFactory>();
            }
        }

        [Test]
        public void TestGetUserById()
        {
            HDUserDataAccess testObject = new HDUserDataAccessForTests();
            
            string userIdcs = "adsf";
            
            DbUser user = testObject.GetUserById(userIdcs);

            Assert.AreEqual(1, Fakes.DataAccessFakeFactory.commands.Count);

            DataAccessFakeFactory.commands[0].Received(1).CreateParameter();
            Assert.AreEqual(1, DataAccessFakeFactory.commands[0].Parameters.Count);
            
            Assert.AreEqual(IDCS_COLUMN_NAME, DataAccessFakeFactory.parameters[0].ParameterName);
            Assert.AreEqual(userIdcs, DataAccessFakeFactory.parameters[0].Value);

            string queryExpected = string.Format("SELECT * FROM {0} WHERE {1} LIKE :{1}", TABLE_NAME, IDCS_COLUMN_NAME);
            Assert.AreEqual(queryExpected, DataAccessFakeFactory.commands[0].CommandText);
        }

        [Test]
        public void TestGetUserByName()
        {
            HDUserDataAccessForTests testObject = new HDUserDataAccessForTests();
            
            string userName = "username";
            DbUser user = testObject.GetUserByName(IdentityData.TEST_HELPDESK_UNIQUE_NAME, userName);

            Assert.AreEqual(1, DataAccessFakeFactory.commands.Count);

            DataAccessFakeFactory.commands[0].Received(2).CreateParameter();
            Assert.AreEqual(2, DataAccessFakeFactory.commands[0].Parameters.Count);
            Assert.AreEqual(USERNAME_COLUMN_NAME, DataAccessFakeFactory.parameters[0].ParameterName);
            Assert.AreEqual(userName, DataAccessFakeFactory.parameters[0].Value);

            Assert.AreEqual(HELPDESKUNIQUENAME_COLUMN_NAME, DataAccessFakeFactory.parameters[1].ParameterName);
            Assert.AreEqual(IdentityData.TEST_HELPDESK_UNIQUE_NAME, DataAccessFakeFactory.parameters[1].Value);

            string queryExpected = string.Format("SELECT * FROM {0} WHERE {1} LIKE :{1} AND {2} LIKE :{2}", TABLE_NAME, USERNAME_COLUMN_NAME, HELPDESKUNIQUENAME_COLUMN_NAME);
            Assert.AreEqual(queryExpected, DataAccessFakeFactory.commands[0].CommandText);
        }

        private const string IDCS_COLUMN_NAME = "idcs";
        private const string TABLE_NAME = "endusers";
        private const string USERNAME_COLUMN_NAME = "signature";
        private const string HELPDESKUNIQUENAME_COLUMN_NAME = "helpdesk_unique_name";
    }
}
