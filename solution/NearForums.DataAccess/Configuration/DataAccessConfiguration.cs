using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace NearForums.DataAccess.Configuration
{
    public class DataAccessConfiguration : ConfigurationSection
    {
        [Serializable]
        public enum GetRowsAffected
        {
            ExecuteScalar,
            ExecuteNonQuery
        }

        private const string GET_ROWS_AFFECTED_METHOD_ATTRIBUTENAME = "getRowsAffectedMethod";

        [ConfigurationProperty(
            GET_ROWS_AFFECTED_METHOD_ATTRIBUTENAME, 
            DefaultValue= GetRowsAffected.ExecuteNonQuery, 
            IsRequired= false)]
        public GetRowsAffected GetRowsAffectedMethod
        {
            get
            {
                return (GetRowsAffected)this[GET_ROWS_AFFECTED_METHOD_ATTRIBUTENAME];
            }
            set
            {
                this[GET_ROWS_AFFECTED_METHOD_ATTRIBUTENAME] = value;
            }
        }
    }
}
