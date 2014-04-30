using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace NearForums.Helpdesk.DataAccess.Data
{
    public static class SystemDataExtensions
    {
        public static TResult SafelyExecuteScalar<TResult>(this DbCommand cmd)
            where TResult : struct
        {
            object resultObject = default(TResult);

            cmd.WithConnectionOpen(
                () => { resultObject = cmd.ExecuteScalar(); });

            TResult result = (TResult)resultObject;

            return result;
        }

        public static TResult SafelyExecute<TResult>(this DbCommand cmd)
            where TResult : class, new()
        {
            TResult result = null;
            cmd.WithConnectionOpen(
                () => { result = cmd.GetSingleRow<TResult>(); });

            return result;
        }

        public static void SafelyExecute(this DbCommand cmd)
        {
            cmd.WithConnectionOpen(
                () => { cmd.ExecuteNonQuery(); });
        }

        public static TResult GetSingleRow<TResult>(this DbCommand cmd)
            where TResult: class, new() 
        {
            TResult result = null;
            DbDataReader reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                if (result != null)
                {
                    throw new Exception("Query returned more than one row");
                }

                result = ReadColumnsFromCurrentRow<TResult>(reader);
            }

            return result;
        }

        private static void WithConnectionOpen(this DbCommand cmd, Action action)
        {
            cmd.Connection.Open();

            try
            {
                action();
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        private static TResult ReadColumnsFromCurrentRow<TResult>(DbDataReader reader)
            where TResult : new()
        {
            TResult result = new TResult();

            foreach (PropertyInfo property in result.GetType().GetProperties())
            {
                object[] attributes = (DataMemberAttribute[])property.GetCustomAttributes(typeof(DataMemberAttribute), true);

                if (attributes == null || attributes.Length == 0)
                {
                    continue;
                }

                if (attributes.Length > 1)
                {
                    throw new Exception(string.Format("Property has multiple {0} attributes", typeof(DataMemberAttribute).Name));
                }

                string dataMemberName = ((DataMemberAttribute)attributes[0]).Name;

                property.SetValue(result, reader[dataMemberName], null);
            }

            return result;
        }
    }
}
