using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Tests.Unit.DataAccess
{
    public class DataAccessTestsBase
    {
        [SetUp]
        public void SetUp()
        {
            Fakes.DataAccessFakeFactory.CleanUp();
        }

        protected static ConnectionStringSettings GetSettings()
        {
            ConnectionStringSettings connectionStringSettings = new ConnectionStringSettings(CONNECTION_STRING_NAME, CONNECTION_STRING, PROVIDER_NAME);

            return connectionStringSettings;
        }

        private const string CONNECTION_STRING_NAME = "testConnectinoString";
        private const string CONNECTION_STRING = "abc";
        private const string PROVIDER_NAME = "testProvider";
    }
}
