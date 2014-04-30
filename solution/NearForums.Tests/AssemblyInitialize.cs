using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NearForums.Configuration;
using NearForums.Configuration.Settings;
using NearForums.Helpdesk.DataAccess.Model;
using System.Data.Common;
using NSubstitute;
using NearForums.Helpdesk.Base.Principal;
using System.Configuration;
using NearForums.Helpdesk.Configuration;

namespace NearForums.Tests
{
	[TestClass]
	public static class InitTestEnv
	{
		[AssemblyInitialize]
		public static void Initialize(TestContext context) 
		{
            NearForums.Helpdesk.DataProvider.Factory.Instance = new DataProviderFactory();

            NearForums.Helpdesk.Modules.DbMaintenance maintenanceModule = new NearForums.Helpdesk.Modules.DbMaintenance();
            NearForums.Helpdesk.DataAccess.Data.NFMaintenanceDataAccess nearForumDbAccess
                = new NearForums.Helpdesk.DataAccess.Data.NFMaintenanceDataAccess(
                    System.Configuration.ConfigurationManager.ConnectionStrings[FORUM_CONNECTION_STRING_NAME],
                    TEST_HELPDESK_NAME);

            nearForumDbAccess.EnsureProperSchemaExists();
            LoadFakeNearForumSiteConfiguration();
		}

        [AssemblyCleanup]
        public static void Cleanup()
        {
            InitTestEnv.DropTestSchema();
            InitTestEnv.ClearLuceneIndex();
        }

        private static void ClearLuceneIndex()
        {
            System.IO.DirectoryInfo luceneParentDir = new System.IO.DirectoryInfo( SiteConfiguration.Current.Search.IndexPath );

            if (!(SiteConfiguration.Current.GetType().Equals(typeof(NearForums.Configuration.SiteConfiguration))))
            {
                luceneParentDir = luceneParentDir.Parent;
            }

            foreach (System.IO.DirectoryInfo dir in luceneParentDir.GetDirectories())
            {
                ClearLuceneIndex(dir);
            }
        }

        private static void ClearLuceneIndex(System.IO.DirectoryInfo luceneDir)
        {
            try
            {
                using (Lucene.Net.Store.Directory dir
                    = Lucene.Net.Store.FSDirectory.Open(
                        luceneDir))
                {
                    dir.ClearLock("write.lock");

                    using (
                        Lucene.Net.Index.IndexWriter writer = new Lucene.Net.Index.IndexWriter(
                            dir,
                            null,
                            Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED))
                    {
                        writer.DeleteAll();
                        writer.Commit();
                    }
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Cannot clear lucenen's index '{0} : {1}'", luceneDir.FullName, exc.Message);
            }
        }

        private static void LoadFakeNearForumSiteConfiguration()
        {
            NearForumConfigOverriden config = (NearForumConfigOverriden)ConfigurationManager.GetSection("site");
            config.Search.Context = Substitute.For<NearForums.Helpdesk.Base.ContextWrapper.IHttpContext>();

            var fakePrincipal = Substitute.For<System.Security.Principal.IPrincipal>();
            config.Search.Context.User.Returns(fakePrincipal);

            var fakeIdentity = Substitute.For<IHelpdeskIdentity>();
            fakeIdentity.HelpdeskUniqueName.Returns(TEST_HELPDESK_NAME);
            fakeIdentity.IsAuthenticated.Returns(true);
            fakePrincipal.Identity.Returns(fakeIdentity);

            NearForums.Configuration.SiteConfiguration.Current = config;
        }

        private class DataProviderFactory : NearForums.Helpdesk.DataProvider.Factory
        {
            public DataProviderFactory()
            {
                base.httpContext = NSubstitute.Substitute.For<NearForums.Helpdesk.Base.ContextWrapper.IHttpContext>();

                IHelpdeskIdentity testtIdentity = Substitute.For<IHelpdeskIdentity>();
                base.httpContext.User.Identity.Returns(testtIdentity);
                base.httpContext.User.Identity.IsAuthenticated.Returns(true);
                ((IHelpdeskIdentity)base.httpContext.User.Identity).HelpdeskUniqueName.Returns(TEST_HELPDESK_NAME);
            }
        }

        private static void DropTestSchema()
        {
            DbCommand cmd = NearForums.Helpdesk.DataProvider.Factory.Instance.CreateCommand();
            cmd.Connection = NearForums.Helpdesk.DataProvider.Factory.Instance.CreateConnection();
            cmd.Connection.ConnectionString =
                System.Configuration.ConfigurationManager.ConnectionStrings[FORUM_CONNECTION_STRING_NAME].ConnectionString;

            try
            {
                cmd.Connection.Open();
                cmd.CommandText = string.Format("DROP SCHEMA {0} CASCADE;", TEST_HELPDESK_NAME);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd.Connection.State == System.Data.ConnectionState.Open)
                {
                    cmd.Connection.Close();
                }
            }
        }

        private const string TEST_HELPDESK_NAME = "testHelpdesk";
        private const string FORUM_CONNECTION_STRING_NAME = "Forums";
	}
}
