using NearForums.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearForums.Helpdesk.Configuration
{
    public class NearForumConfigOverriden : SiteConfiguration
    {
        [ConfigurationProperty(SEARCH)]
        public new HDSearchElement Search
        {
            get
            {
                return (HDSearchElement)this[SEARCH];
            }
            set
            {
                this[SEARCH] = value;
                value.ParentElement = this;
            }
        }

        private const string SEARCH = "search";
    }
}
