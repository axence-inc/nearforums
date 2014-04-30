using NearForums.Configuration;
using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.Base.Principal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NearForums.Helpdesk.Configuration
{
    public class HDSearchElement : SearchElement
    {
        public override string IndexPath
        {
            get
            {
                return Path.Combine(base.IndexPath, GetHelpdeskName());
            }
        }

        public IHttpContext Context
        {
            get
            {
                return context ?? HTTPContextWrapper.Current;
            }
            set
            {
                this.context = value;
            }
        }

        private string GetHelpdeskName()
        {
            string helpdeskName;

            if (this.Context.User == null || this.Context.User.Identity == null || !this.Context.User.Identity.IsAuthenticated)
            {
                throw new ConfigurationErrorsException("Cannot get helpdeskname if user not authenticated");
            }

            if (!( this.Context.User.Identity is IHelpdeskIdentity))
            {
                throw new ConfigurationErrorsException("Invalid identity type : " + this.Context.User.Identity.GetType().FullName);
            }

            IHelpdeskIdentity helpdeskIdentity = (IHelpdeskIdentity)this.Context.User.Identity;

            helpdeskName = helpdeskIdentity.HelpdeskUniqueName;

            return helpdeskName;
        }

        private IHttpContext context;
    }
}
