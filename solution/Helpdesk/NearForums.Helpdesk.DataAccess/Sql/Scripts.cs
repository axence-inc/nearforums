using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NearForums.Helpdesk.DataAccess.Sql
{
    internal class Scripts
    {
        public static StringBuilder GetScript()
        {
            return new MultipleStreamsReader<string>(
                    resourceName => ResourceNameToStream(resourceName),
                    GetOrderedResources())
                .ReadToEnd();
        }

        private static Stream ResourceNameToStream(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        private static string[] GetOrderedResources()
        {
            string prefix = typeof(Scripts).FullName;
            prefix = prefix.Substring(0, prefix.LastIndexOf('.'));

            string[] orderedResource = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(resName => resName.StartsWith(prefix) && resName.EndsWith(".sql"))
                .OrderBy(resName => resName)
                .ToArray();

            return orderedResource;
        }


    }
}
