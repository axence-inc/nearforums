using NearForums.Helpdesk.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NearForums.Helpdesk.DataAccess.Data
{
    public class HDUserDataAccess : BaseDataAccess, NearForums.Helpdesk.DataAccess.Data.IHDUserDataAccess
    {
        public HDUserDataAccess(ConnectionStringSettings connectionString)
            :base(connectionString)
        {
        }

        public DbUser GetUserByName(string helpdeskUniqueName, string username)
        {
            DbUser user = this
                .CreateGetByParamCommand<DbUser, string, string>(u => u.Name, username, u => u.HelpdeskUniqueName, helpdeskUniqueName)
                .SafelyExecute<DbUser>();

            return user;
        }

        public DbUser GetUserById(string idcs)
        {
            DbUser user = this
                .CreateGetByParamCommand<DbUser, object>(u => u.Idcs, idcs)
                .SafelyExecute<DbUser>();

            return user;
        }
    }
}
