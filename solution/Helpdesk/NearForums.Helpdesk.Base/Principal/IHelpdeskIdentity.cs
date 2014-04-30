using System;
using System.Security.Principal;

namespace NearForums.Helpdesk.Base.Principal
{
    public interface IHelpdeskIdentity : IIdentity
    {
        string HelpdeskUniqueName { get; set; }
    }
}
