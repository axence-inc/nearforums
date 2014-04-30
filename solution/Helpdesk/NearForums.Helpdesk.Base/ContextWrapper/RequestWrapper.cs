using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace NearForums.Helpdesk.Base.ContextWrapper
{
    internal class RequestWrapper : IRequest
    {
        public RequestWrapper(HttpContext context)
        {
            this.context = context;
        }

        public string CurrentExecutionFilePathExtension
        {
            get
            {
                return this.context.Request.CurrentExecutionFilePathExtension;
            }
        }

        public System.Collections.Specialized.NameValueCollection Headers
        {
            get { return this.context.Request.Headers; }
        }

        public Uri Url
        {
            get { return context.Request.Url; }
        }

        private HttpContext context { get; set; }
    }

}
