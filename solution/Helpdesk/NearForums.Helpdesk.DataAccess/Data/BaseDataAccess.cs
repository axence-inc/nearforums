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
    public abstract class BaseDataAccess
    {
        public BaseDataAccess(ConnectionStringSettings connectionString)
        {
            this.connectionStringSettings = connectionString;
        }

        protected DbProviderFactory GetDbProviderFactory(string providerInvariantName)
        {
            return this.dbProviderFactoryDefault ?? DbProviderFactories.GetFactory(providerInvariantName);
        }

        protected DbCommand CreateGetByParamCommand<TTable, TTableKey, TTableKey2>(
            Expression<Func<TTable, TTableKey>> keySelector, TTableKey keyValue,
            Expression<Func<TTable, TTableKey2>> keySelector2, TTableKey keyValue2)
        {
            DbCommand cmd = CreateGetByParamCommand<TTable, TTableKey>(keySelector, keyValue);

            string keyColumnName = ModelExtensions.GetColumnName<TTable, TTableKey2>(keySelector2);

            cmd.CommandText += string.Format(" AND {0} LIKE :{0}", keyColumnName);
            
            DbParameter param = cmd.CreateParameter();
            param.ParameterName = keyColumnName;
            param.Value = keyValue2;

            cmd.Parameters.Add(param);

            return cmd;
        }

        protected DbCommand CreateGetByParamCommand<TTable, TTableKey>(Expression<Func<TTable, TTableKey>> keySelector, TTableKey keyValue)
        {
            string tableName = ModelExtensions.GetTableName<TTable>();
            string keyColumnName = ModelExtensions.GetColumnName<TTable, TTableKey>(keySelector);

            DbCommand cmd = GetCommand();
            cmd.CommandText = string.Format("SELECT * FROM {0} WHERE {1} LIKE :{1}",
                tableName,
                keyColumnName);

            DbParameter param = cmd.CreateParameter();
            param.ParameterName = keyColumnName;
            param.Value = keyValue;

            cmd.Parameters.Add(param);

            return cmd;
        }

        protected DbCommand GetCommand()
        {
            return this.GetCommand(this.connectionStringSettings);
        }

        protected DbProviderFactory dbProviderFactoryDefault = null;

        private DbCommand GetCommand(ConnectionStringSettings connectionStringSettings)
        {
            string factoryName = connectionStringSettings.ProviderName;

            if (string.IsNullOrEmpty(factoryName))
            {
                throw new ConfigurationErrorsException("ProviderName not provided for " + connectionStringSettings.Name);
            }

            DbProviderFactory factory = this.GetDbProviderFactory(factoryName);

            DbCommand cmd = factory.CreateCommand();
            cmd.Connection = factory.CreateConnection();
            cmd.Connection.ConnectionString = connectionStringSettings.ConnectionString;

            return cmd;
        }
        
        private ConnectionStringSettings connectionStringSettings;
    }
}
