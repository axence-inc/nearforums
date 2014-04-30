using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.DataAccess.Data;
using NearForums.Helpdesk.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace NearForums.Helpdesk
{
    public class HelpdeskMembershipProvider: System.Web.Security.MembershipProvider
    {
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override System.Web.Security.MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out System.Web.Security.MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Web.Security.MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override System.Web.Security.MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override System.Web.Security.MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override System.Web.Security.MembershipUser GetUser(string username, bool userIsOnline)
        {
            System.Security.Principal.IPrincipal principal = this.HttpContext.User;
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
            {
                throw new SecurityException("User not authenticated");
            }

            if (!(principal.Identity is HDMembershipUser))
            {
                throw new SecurityException("Authenticated user of incorect type");
            }

            HDMembershipUser hdUserAuthenticated = (HDMembershipUser)(principal.Identity);

            DbUser dbUser = UserDataAccess.GetUserByName(hdUserAuthenticated.HelpdeskUniqueName, username);
            MembershipUser user = HDMembershipUser.FromUser(dbUser, this.Name);
            
            return user;
        }

        public override System.Web.Security.MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            DbUser dbUser = this.UserDataAccess.GetUserById((string)providerUserKey);
            MembershipUser user = HDMembershipUser.FromUser(dbUser, this.Name);

            return user;
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override System.Web.Security.MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(System.Web.Security.MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            throw new NotImplementedException();
            //DbUser user = new HDUserDataAccess(this.ConnectionStringSettings).GetUserByName(username);

            //return user != null && user.ValidatePass(password);
        }

        public IHttpContext HttpContext
        {
            get
            {
                return this.httpContext ?? HTTPContextWrapper.Current;
            }
            protected set
            {
                this.httpContext = value;
            }
        }

        public IHelpdeskConfig Config
        {
            get
            {
                return this.config ?? HelpdeskConfig.Config;
            }
            protected set
            {
                this.config = value;
            }
        }

        protected IHDUserDataAccess UserDataAccess
        {
            get
            {
                return this.hdUserDataAccess ?? new NearForums.Helpdesk.DataAccess.Data.HDUserDataAccess(this.ConnectionStringSettings);
            }
            set
            {
                this.hdUserDataAccess = value;
            }
        }

        private IHDUserDataAccess hdUserDataAccess;
        private IHelpdeskConfig config;
        private IHttpContext httpContext;

        private ConnectionStringSettings ConnectionStringSettings { get { return HelpdeskConfig.Config.HdConnectionStringSettings; } }
    }
}
