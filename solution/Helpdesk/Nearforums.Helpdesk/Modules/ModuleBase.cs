using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NearForums.Helpdesk.Base.ContextWrapper;

namespace NearForums.Helpdesk.Modules
{
    public abstract partial class ModuleBase : System.Web.IHttpModule
    {
        public abstract void Dispose();

        public abstract void Init(System.Web.HttpApplication context);

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

        protected bool SkipCurrentRequest()
        {
            string requestFileExt = this.HttpContext.Request.CurrentExecutionFilePathExtension;
            bool skipCurrent = this.Config.SkipFilesArray.Contains(requestFileExt);

            return skipCurrent;
        }

        private IHttpContext httpContext;
        private IHelpdeskConfig config;
    }
}
