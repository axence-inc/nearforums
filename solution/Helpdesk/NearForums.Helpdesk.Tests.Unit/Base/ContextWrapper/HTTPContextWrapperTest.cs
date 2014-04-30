using NearForums.Helpdesk.Base.ContextWrapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NearForums.Helpdesk.Tests.Unit.Base.ContextWrapper
{
    public class HTTPContextWrapperTest
    {
        [Test]
        public void ChangingUserInWrapperShouldChangeUserInUnderlyingContext()
        {
            HTTPContextWrapperForTests contextWrapper = new HTTPContextWrapperForTests();
            Assert.AreEqual(contextWrapper.User, contextWrapper.GetHttpContext().User);

            contextWrapper.User = new GenericPrincipal(
                new GenericIdentity("test"), 
                new string[] { "testRole" });
            Assert.AreEqual(contextWrapper.User, contextWrapper.GetHttpContext().User);
        }

        [Test]
        public void RequestFromWrapperAndUnderlyingContextHaveToBeEqual()
        {
            HTTPContextWrapperForTests contextWrapper = new HTTPContextWrapperForTests();
            
            Assert.AreEqual(contextWrapper.Request.CurrentExecutionFilePathExtension, contextWrapper.GetHttpContext().Request.CurrentExecutionFilePathExtension);
            Assert.AreEqual(contextWrapper.Request.Headers, contextWrapper.GetHttpContext().Request.Headers);
            Assert.AreEqual(contextWrapper.Request.Url, contextWrapper.GetHttpContext().Request.Url);
        }

        private class HTTPContextWrapperForTests : HTTPContextWrapper
        {
            public HTTPContextWrapperForTests()
                : base(GetUnderlyingContext())
            { }

            public HttpContext GetHttpContext()
            {
                return base.context;
            }

            private static System.Web.HttpContext GetUnderlyingContext()
            {
                HttpContext context = new HttpContext(
                    new HttpRequest("aFile", "an://url", "aQueryString"),
                    new HttpResponse(new StreamWriter(new MemoryStream())));

                return context;
            }
        }
    }
}
