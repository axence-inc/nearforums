using NearForums.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearForums.Tests.TestCleanup
{
    partial class Cleaner
    {
        private interface ICleanerDataAccess<TModel>
        {
            bool PermanentlyDelete(TModel obj);
        }

        private abstract class BaseCleanerDataAccess<TModel, TDataAccess, TKey> : ICleanerDataAccess<TModel>
            where TDataAccess : DataAccess.BaseDataAccess, new()
        {
            protected abstract string TableName { get; }
            
            protected abstract string KeyColumnName { get; }

            protected abstract TKey GetKeyValue(TModel obj);

            public virtual bool PermanentlyDelete(TModel obj)
            {
                TDataAccess dataAccess = new TDataAccess();

                return this.PermanentlyDelete(dataAccess, obj);
            }

            protected bool PermanentlyDelete(BaseDataAccess bda, TModel obj)
            {
                System.Data.Common.DbCommand cmd = GetDeletePermanentlyCommand<TKey>(bda, this.TableName, this.KeyColumnName, this.GetKeyValue(obj));

                return 0 < cmd.SafeExecuteAndGetNoOfRowsAffected();
            }

            protected System.Data.Common.DbCommand GetDeletePermanentlyCommand<T>(BaseDataAccess bda, string tableName, string columnName, T value)
            {
                System.Data.Common.DbCommand cmd = bda.Factory.CreateCommand();
                cmd.Connection = bda.GetConnection();

                string commandText = string.Format("DELETE FROM {0} WHERE {1} = :{1}", tableName, columnName); ;
                cmd.CommandText = commandText;

                cmd.AddParameter<T>(bda.Factory, columnName, value);

                return cmd;
            }
        }
    }
}
