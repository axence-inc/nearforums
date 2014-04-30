using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Security;

namespace NearForums.Helpdesk.DataAccess.Model
{
    internal static class ModelExtensions
    {
        internal static string GetColumnName<TModelObj, TPropertyType>(Expression<Func<TModelObj, TPropertyType>> propertySelector)
        {
            PropertyInfo pi = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
            string columnName = pi.GetSignleAttribute<DataMemberAttribute>(true).Name;

            return columnName;
        }

        internal static string GetTableName<TModelObj>()
        {
            string tableName = typeof(TModelObj).GetSignleAttribute<DataContractAttribute>(false).Name;
            
            return tableName;
        }

        private static TAttr GetSignleAttribute<TAttr>(this ICustomAttributeProvider provider, bool inherit)
        {
            object[] attributes = provider.GetCustomAttributes(typeof(TAttr), inherit);

            if (attributes.Length != 1)
            {
                throw new Exception(
                    string.Format(
                        "{0} - invalid number of attributes of type {1} : {2}",
                        provider.GetType().Name,
                        typeof(DataContractAttribute).Name,
                        attributes.Length));
            }

            TAttr attr = (TAttr)attributes[0];

            return attr;
        }
    }
}
