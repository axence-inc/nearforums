using System;
namespace NearForums.Helpdesk.DataAccess.Data
{
    public interface IHDUserDataAccess
    {
        NearForums.Helpdesk.DataAccess.Model.DbUser GetUserById(string idcs);
        NearForums.Helpdesk.DataAccess.Model.DbUser GetUserByName(string helpdeskUniqueName, string username);
    }
}
