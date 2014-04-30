using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearForums.Helpdesk.Base.ContextWrapper
{
    public class HTTPContextWrapper : IHttpContext
    {
        public static HTTPContextWrapper Current
        {
            get
            {
                HTTPContextWrapper value = new HTTPContextWrapper(System.Web.HttpContext.Current);

                return value;
            }
        }

        public IRequest Request { get; private set; }

        public System.Security.Principal.IPrincipal User 
        {
            get { return context.User; }
            set { this.context.User = value; }
        }

        protected HTTPContextWrapper(System.Web.HttpContext context)
        {
            this.context = context;

            this.Request = new RequestWrapper(context);
        }

        protected System.Web.HttpContext context;
    }
}
