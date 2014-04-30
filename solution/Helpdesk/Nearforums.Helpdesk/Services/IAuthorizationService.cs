using System;
namespace NearForums.Helpdesk.Services
{
    public interface IAuthorizationService
    {
        void Authorize(string authString);
    }
}
