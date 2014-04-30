using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearForums.Helpdesk.Base.ContextWrapper
{
    public interface IHttpContext
    {
        IRequest Request { get; }

        System.Security.Principal.IPrincipal User { get; set; }
    }
}
