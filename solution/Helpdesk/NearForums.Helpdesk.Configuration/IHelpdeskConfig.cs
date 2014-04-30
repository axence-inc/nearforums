using System;
namespace NearForums.Helpdesk.Configuration
{
    public interface IHelpdeskConfig
    {
        string CookieName { get; set; }
        bool Debug { get; set; }
        System.Configuration.ConnectionStringSettings ForumConnectionStringSettings { get; }
        System.Configuration.ConnectionStringSettings HdConnectionStringSettings { get; }
        string[] SkipFilesArray { get; }

        string SecretToken1 { get; }
        string SecretToken2 { get; }
    }
}
