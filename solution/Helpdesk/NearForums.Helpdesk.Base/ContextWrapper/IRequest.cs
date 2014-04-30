using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace NearForums.Helpdesk.Base.ContextWrapper
{
    public interface IRequest
    {
        string CurrentExecutionFilePathExtension { get; }
        NameValueCollection Headers { get; }
        Uri Url { get; }
    }
}
