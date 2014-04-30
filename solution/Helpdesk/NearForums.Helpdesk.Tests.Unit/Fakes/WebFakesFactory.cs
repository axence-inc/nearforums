using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk.Base.ContextWrapper;
using NearForums.Helpdesk.DataAccess.Data;
using NearForums.Helpdesk.DataAccess.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NearForums.Helpdesk.Base.Principal;

namespace NearForums.Helpdesk.Tests.Unit.Fakes
{
    public static class WebFakesFactory
    {
        public static T Create<T>()
            where T : class
        {
            T value;

            if (ImplementsInterface<IPrincipal, T>())
            {
                value = (T)WebFakesFactory.CreateHelpdeskPrincipal();
            }
            else if (ImplementsInterface<IHelpdeskConfig, T>())
            {
                value = (T)WebFakesFactory.CreateConfig();
            }
            else if (ImplementsInterface<IHttpContext, T>())
            {
                value = (T)WebFakesFactory.CreateContext();
            }
            else if (ImplementsInterface<System.Web.Security.MembershipProvider, T>())
            {
                value = WebFakesFactory.CreateMembershipProvider() as T;
                if (value == null)
                {
                    throw new Exception("This shouldn't happen. It really shouldn't.");
                }
            }
            else if (ImplementsInterface<IHDUserDataAccess, T>())
            {
                value = (T)WebFakesFactory.CreateHDUserDataAccess();
            }
            else
            {
                throw new ArgumentException("No factry for given type : " + typeof(T).FullName);
            }

            return value;
        }

        private static IHDUserDataAccess CreateHDUserDataAccess()
        {
            IHDUserDataAccess dataAccess = Substitute.For<IHDUserDataAccess>();

            return dataAccess;
        }

        private static bool ImplementsInterface<TInterface, TObject>()
        {
            return typeof(TInterface).IsAssignableFrom(typeof(TObject));
        }

        private static IPrincipal CreateHelpdeskPrincipal()
        {
            IPrincipal principal = Substitute.For<IPrincipal>();

            IIdentity identity = CreateIdentity();

            principal.Identity.Returns(identity);

            return principal;
        }

        private static IHelpdeskIdentity CreateIdentity()
        {
            IHelpdeskIdentity identity = Substitute.For<IHelpdeskIdentity>();

            identity.HelpdeskUniqueName.Returns(IdentityData.TEST_HELPDESK_UNIQUE_NAME);

            return identity;
        }

        private static IHelpdeskConfig CreateConfig()
        {
            IHelpdeskConfig config = Substitute.For<IHelpdeskConfig>();
            
            config.SkipFilesArray.Returns(IdentityData.SkipExtensions);
            config.CookieName = "authCookie";
            config.SecretToken1.Returns(IdentityData.SecretToken.SECRET_TOKEN_1);
            config.SecretToken2.Returns(IdentityData.SecretToken.SECRET_TOKEN_2);

            return config;
        }

        private static IHttpContext CreateContext()
        {
            IHttpContext context = Substitute.For<IHttpContext>();
            
            IRequest request = CreateRequest();
            context.Request.Returns(request);

            return context;
        }

        private static IRequest CreateRequest()
        {
            IRequest request = Substitute.For<IRequest>();
            
            var headers = new System.Collections.Specialized.NameValueCollection();
            request.Headers.Returns(headers);

            return request;
        }

        private static System.Web.Security.MembershipProvider CreateMembershipProvider()
        {
            System.Web.Security.MembershipProvider provider = Substitute.For<System.Web.Security.MembershipProvider>();

            return provider;
        }
        
    }
}
