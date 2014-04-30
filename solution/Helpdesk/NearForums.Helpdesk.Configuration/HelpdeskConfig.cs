using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Configuration
{
    public class HelpdeskConfig : ConfigurationSection, NearForums.Helpdesk.Configuration.IHelpdeskConfig
    {
        public static HelpdeskConfig Config
        {
            get
            {
                object section = ConfigurationManager.GetSection(CONFIG_SECTION_NAME);

                if (section == null)
                {
                    throw new ConfigurationErrorsException(
                        string.Format(
                            "Config section '{0}' not found",
                            CONFIG_SECTION_NAME));
                }

                HelpdeskConfig value = (HelpdeskConfig)section;
                
                return value;
            }
        }

        [ConfigurationProperty(COOKIE_NAME, DefaultValue = "x-axence-helpdesk-apikey", IsRequired = true)]
        public string CookieName
        {
            get
            {
                return (string)this[COOKIE_NAME];
            }
            set
            {
                this[COOKIE_NAME] = value;
            }
        }

        public string[] SkipFilesArray
        {
            get
            {
                if (this.skipFilesArray == null)
                {
                    this.skipFilesArray = this.SkipFiles.Split(skipFilesSeparators, StringSplitOptions.RemoveEmptyEntries);
                }

                return this.skipFilesArray;
            }
        }
        
        [ConfigurationProperty(DEBUG, DefaultValue = false, IsRequired = false)]
        public bool Debug
        {
            get
            {
                return (bool)this[DEBUG];
            }
            set
            {
                this[DEBUG] = value;
            }
        }

        public ConnectionStringSettings HdConnectionStringSettings
        {
            get
            {
                ConnectionStringSettings hdConnectionString = ConfigurationManager.ConnectionStrings[this.HdConnectinoStringName];

                if (hdConnectionString == null)
                {
                    throw new ConfigurationErrorsException("Connectino string not found : " + this.HdConnectinoStringName);
                }

                return hdConnectionString;
            }
        }

        public ConnectionStringSettings ForumConnectionStringSettings
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[this.ForumConnectionStringName];
            }
        }

        [ConfigurationProperty(SECRECT_TOKEN_1, DefaultValue="ad37e5ab-ac25-44ca-8cd5-57a5a1dee9f6")]
        public string SecretToken1
        {
            get 
            {
                return (string)this[SECRECT_TOKEN_1];
            }
        }

        [ConfigurationProperty(SECRECT_TOKEN_2, DefaultValue="3c6b5429-08fc-4937-9f8a-ee481cfc91fc")]
        public string SecretToken2
        {
            get 
            {
                return (string)this[SECRECT_TOKEN_2];
            }
        }

        [ConfigurationProperty(HD_CONNECTION_STRING_NAME, IsRequired=true)]
        private string HdConnectinoStringName 
        { 
            get { return (string)this[HD_CONNECTION_STRING_NAME]; }
            set { this[HD_CONNECTION_STRING_NAME] = value; }
        }

        [ConfigurationProperty(FORUM_CONNECTION_STRING_NAME, IsRequired = true)]
        private string ForumConnectionStringName
        {
            get { return (string)this[FORUM_CONNECTION_STRING_NAME]; }
            set { this[FORUM_CONNECTION_STRING_NAME] = value; }
        }

        [ConfigurationProperty(SKIP_FILES, DefaultValue = "*.js, *.css, *.html", IsRequired = false)]
        protected string SkipFiles
        {
            get
            {
                return (string)this[SKIP_FILES];
            }
            set
            {
                this[SKIP_FILES] = value;
            }
        }

        private string[] skipFilesArray = null;

        private char[] skipFilesSeparators = new char[] { ',', ' ' };

        private const string CONFIG_SECTION_NAME = "helpdesk";

        private const string SECRECT_TOKEN_1 = "secretToken1";
        private const string SECRECT_TOKEN_2 = "secretToken2";
        private const string COOKIE_NAME = "cookieName";
        private const string SKIP_FILES = "skipFiles";
        private const string HD_CONNECTION_STRING_NAME = "hdConnectionStringName";
        private const string FORUM_CONNECTION_STRING_NAME = "forumConnectionStringName";
        private const string DEBUG = "debug";
    }
}
