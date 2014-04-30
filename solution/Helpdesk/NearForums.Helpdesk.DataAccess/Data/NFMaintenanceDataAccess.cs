using NearForums.Helpdesk.DataAccess.Sql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace NearForums.Helpdesk.DataAccess.Data
{
    public class NFMaintenanceDataAccess : BaseDataAccess, NearForums.Helpdesk.DataAccess.Data.INFMaintenanceDataAccess
    {
        public NFMaintenanceDataAccess(
            ConnectionStringSettings forumConnectionStringSettings, string schemaName)
            :base(forumConnectionStringSettings)
        {
            this.schemaName = schemaName;
        }

        public void EnsureProperSchemaExists()
        {
            if (!this.CheckSchemaExists())
            {
                this.CreateSchema();
            }
        }

        private bool CheckSchemaExists()
        {
            bool exists = false;

            try
            {
                DbCommand cmd = base.GetCommand();
                cmd.CommandText = TEST_EXISTS_QUERY;
                cmd.SafelyExecute();
                exists = true;
            }
            catch 
            {
            }

            return exists;
        }

        private void CreateSchema()
        {
            DbCommand cmd = base.GetCommand();
            cmd.CommandText = GetCreateDbScript();

            cmd.SafelyExecute();
        }

        private string GetCreateDbScript()
        {
            StringBuilder sbCreateDbQuery = new StringBuilder()
                .AppendFormat("CREATE SCHEMA {0};", schemaName);

            sbCreateDbQuery.Append(Scripts.GetScript());

            return sbCreateDbQuery.ToString();
        }

        private string schemaName;

        private const string TEST_EXISTS_QUERY = "SELECT * FROM ForumsCategories LIMIT 1";
    }
}
