using NearForums.Helpdesk.Base.Principal;
using NearForums.Helpdesk.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web.Security;

namespace NearForums.Helpdesk.DataAccess.Model
{
    public class HDMembershipUser : MembershipUser, IHelpdeskIdentity
    {
        public HDMembershipUser(
            string providerName,
            string name,
            object providerUserKey,
            string email,
            string passwordQuestion,
            string comment,
            bool isApproved,
            bool isLockedOut,
            DateTime creationDate,
            DateTime lastLoginDate,
            DateTime lastActivityDate,
            DateTime lastPasswordChangedDate,
            DateTime lastLockoutDate,
            string helpdeskUniqueName)
            : base(providerName, name, providerUserKey, email, passwordQuestion, comment, isApproved, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, lastLockoutDate)
        {
            if (string.IsNullOrEmpty(helpdeskUniqueName))
            {
                throw new ArgumentException("Invalid helpdesk's unique name");
            }

            this.HelpdeskUniqueName = helpdeskUniqueName;
        }

        public string HelpdeskUniqueName { get; set; }

        public static HDMembershipUser FromUser(DbUser user, string providerName)
        {
            HDMembershipUser mu = new HDMembershipUser(
                providerName,
                user.Name,
                user.Idcs,
                user.Email,
                null,
                null,
                true,
                false,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MaxValue,
                user.HelpdeskUniqueName);

            return mu;
        }
        
        #region IIDentity
        public string AuthenticationType
        {
            get { return this.ProviderName; }
        }

        public bool IsAuthenticated
        {
            get;
            set;
        }

        public string Name
        {
            get { return this.UserName; }
        }
        #endregion

        private class Principal : IPrincipal
        {
            public Principal(HDMembershipUser user)
            {
                this.Identity = user;
            }

            public IIdentity Identity
            {
                get;
                private set;
            }

            public bool IsInRole(string role)
            {
                throw new NotImplementedException();
            }
        }

        public System.Security.Principal.IPrincipal ToPrincipal(bool isAuthenticated = false)
        {
            Principal principal = new Principal(this);
            ((HDMembershipUser)principal.Identity).IsAuthenticated = isAuthenticated;

            return principal;
        }
    }
}
