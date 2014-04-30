using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk;
using NearForums.Helpdesk.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.DataAccess.Model;
using NearForums.Helpdesk.Base.Principal;

namespace NearForums.Helpdesk.Modules
{
    public class DbMaintenance : ModuleBase
    {
        public override void Dispose()
        {
        }

        public override void Init(System.Web.HttpApplication context)
        {
            context.PostAuthenticateRequest += context_PostAuthenticateRequest;
        }

        protected INFMaintenanceDataAccess MaintenanceDataAccess
        {
            get
            {
                INFMaintenanceDataAccess value = this.maintenanceDataAccess;

                if (value == null)
                {
                    value = new NFMaintenanceDataAccess(
                        HelpdeskConfig.Config.ForumConnectionStringSettings,
                        ((NearForums.Helpdesk.DataAccess.Model.HDMembershipUser)HTTPContextWrapper.Current.User.Identity).HelpdeskUniqueName);
                }

                return value;
            }
            set
            {
                this.maintenanceDataAccess = value;
            }
        }

        protected void context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            if (!base.SkipCurrentRequest())
            {
                this.CheckDatabaseForSchema();
            }
        }

        private void CheckDatabaseForSchema()
        {
            if (this.HttpContext.User == null || this.HttpContext.User.Identity == null || !this.HttpContext.User.Identity.IsAuthenticated)
            {
                throw new System.Security.Authentication.AuthenticationException();
            }

            if (!(this.HttpContext.User.Identity is IHelpdeskIdentity))
            {
                throw new System.Security.Authentication.AuthenticationException();
            }
                        
            IHelpdeskIdentity user = (IHelpdeskIdentity)this.HttpContext.User.Identity;

            this.MaintenanceDataAccess.EnsureProperSchemaExists();
        }

        private INFMaintenanceDataAccess maintenanceDataAccess;
    }
}
