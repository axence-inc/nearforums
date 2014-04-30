using NearForums.Helpdesk;
using NearForums.Helpdesk.DataAccess.Model;
using NearForums.Helpdesk.Base.ContextWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web.Security;
using NearForums.Helpdesk.Configuration;

namespace NearForums.Helpdesk.Services
{
    public partial class AuthorizationService : NearForums.Helpdesk.Services.IAuthorizationService
    {
        public void Authorize(string authString)
        {
            ClientSideSession session;

            try
            {
                session = ClientSideSession.Get(authString, this.Config.SecretToken1, this.Config.SecretToken2);
            }
            catch (ClientSideSession.InvalidClientSideSession exc)
            {
                throw new System.Security.Authentication.AuthenticationException("Invalid session credentials", exc);
            }

            this.Authorize(session.UserIdcs, session.RememberMe);
        }

        public MembershipProvider MembershipProvider
        {
            get
            {
                MembershipProvider value = this.membershipProvider;

                if (value == null)
                {
                    value = Membership.Provider;
                }

                return value;
            }
            set
            {
                this.membershipProvider = value;
            }

        }

        public IHelpdeskConfig Config
        {
            get
            {
                return this.config ?? HelpdeskConfig.Config;
            }

            set
            {
                this.config = value;
            }
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

        private void Authorize(string userIdcs, bool rememberMe)
        {
            IPrincipal principal = GetPrincipal(userIdcs, rememberMe);

            System.Threading.Thread.CurrentPrincipal = principal;
            this.HttpContext.User = principal;

            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SetAuthCookie(principal.Identity.Name, rememberMe);
            }
        }

        private IPrincipal GetPrincipal(string userIdcs, bool rememberMe)
        {
            IPrincipal authenticatedPrincipal = null;

            MembershipUser membershipUser = this.MembershipProvider.GetUser((object)userIdcs, true);

            if (membershipUser != null)
            {
                HDMembershipUser hdMembershipUser = (HDMembershipUser)membershipUser;
                hdMembershipUser.IsAuthenticated = true;
                authenticatedPrincipal = hdMembershipUser.ToPrincipal(true);
            }
            
            return authenticatedPrincipal;
        }

        private IHelpdeskConfig config;
        private IHttpContext httpContext;
        private MembershipProvider membershipProvider;
    }
}
