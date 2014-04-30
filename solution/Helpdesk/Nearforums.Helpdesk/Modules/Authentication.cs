using NearForums.Helpdesk.Configuration;
using NearForums.Helpdesk.Services;
using NearForums.Helpdesk;
using NearForums.Helpdesk.DataAccess.Data;
using NearForums.Helpdesk.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using NearForums.Helpdesk.Base.ContextWrapper;

namespace NearForums.Helpdesk.Modules
{
    public class Authentication : ModuleBase
    {
        public override void Dispose()
        {
            
        }

        public override void Init(System.Web.HttpApplication context)
        {
            context.AuthenticateRequest += context_AuthenticateRequest;
        }

        protected virtual IAuthorizationService AuthenticationService
        {
            get
            {
                IAuthorizationService service = this.authenticationService;

                if (service == null)
                {
                    service = new AuthorizationService();
                }

                return service;
            }
            set
            {
                this.authenticationService = value;
            }
        }

        protected void context_AuthenticateRequest(object sender, EventArgs e)
        {
            if (!base.SkipCurrentRequest())
            {
                string authString = GetAuthString();
                
                if (string.IsNullOrEmpty(authString))
                {
                    throw new System.Security.Authentication.AuthenticationException("User not authenticated, no authentication string");
                }

                this.TryAuthenticate(authString);
            }
        }

        private void TryAuthenticate(string authString)
        {
            if (!string.IsNullOrEmpty(authString))
            {
                this.AuthenticationService.Authorize(authString);
            }
        }

        private string GetAuthString()
        {
            string cookieName = this.Config.CookieName;

            string cookieValue = null;
            cookieValue = GetCookieValue(cookieName);
            
            return cookieValue;
        }

        private const string QUOTE = "%22";
        private string StripSurroundingQuotes(string str)
        {
            int startIndex = 0;
            int length = str.Length;

            if (str.StartsWith(QUOTE))
            {
                startIndex += QUOTE.Length;
                length -= QUOTE.Length;
            }

            if (str.EndsWith(QUOTE))
            {
                length -= QUOTE.Length;
            }

            str = str.Substring(startIndex, length);

            return str;
        }

        private string GetCookieValue(string cookieName)
        {
            string selectedCookieValue = null;

            string cookies = this.HttpContext.Request.Headers["Cookie"];

            if (!string.IsNullOrEmpty(cookies))
            {
                selectedCookieValue = cookies
                    // separate cookies
                    .Split(cookieSeparators, StringSplitOptions.RemoveEmptyEntries)
                    //each cookie seperate into key-value array
                    .Select<string, string[]>(s => s.Split(cookieKeyValueSeparators, StringSplitOptions.RemoveEmptyEntries))
                    // select cookie by name
                    .Where(arr => arr.Length > 0 && arr[0].Equals(cookieName))
                    // get value & remove quotes
                    .Select<string[], string>(arr => StripSurroundingQuotes(arr[1]))
                    .FirstOrDefault();
            }

            return selectedCookieValue;
        }

        private char[] cookieSeparators = new char[] { ' ', ';' };
        private char[] cookieKeyValueSeparators = new char[] { '=' };

        private IAuthorizationService authenticationService;
    }
}
