using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.Base.Principal;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NearForums.Helpdesk.DataProvider
{
    public class Factory : DbProviderFactory
    {
        public static Factory Instance = new Factory();

        protected Factory ()
	    {
	    }

        public override DbConnection CreateConnection()
        {
            DbConnection connection = NpgsqlFactory.Instance.CreateConnection();
            return connection;
        }

        public override DbCommand CreateCommand()
        {
            NpgsqlCommand cmd = (NpgsqlCommand)NpgsqlFactory.Instance.CreateCommand();

            Command forumCommand = new Command(cmd, GetSchema());

            return forumCommand;
        }

        public override DbParameter CreateParameter()
        {
            return NpgsqlFactory.Instance.CreateParameter();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            NpgsqlDataAdapter adapter = (NpgsqlDataAdapter)NpgsqlFactory.Instance.CreateDataAdapter();

            return adapter;
        }

        private string GetSchema()
        {
            System.Security.Principal.IPrincipal principal = (httpContext == null ? HttpContext.Current.User : httpContext.User);

            if (!principal.Identity.IsAuthenticated)
            {
                throw new AccessDeniedException("Cannot get schema for nonauthenticated user");
            }

            IHelpdeskIdentity membershipUser = (IHelpdeskIdentity)principal.Identity;
            string helpdeskUniqueName = membershipUser.HelpdeskUniqueName;

            return helpdeskUniqueName;
        }

        protected IHttpContext httpContext = null;

        public class AccessDeniedException : Exception
        {
            public AccessDeniedException(string msg)
                : base(msg)
            {
            }
        }
    }
}
