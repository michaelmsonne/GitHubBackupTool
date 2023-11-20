using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GithubBackup.Core;

namespace GithubBackup.Class
{
    internal class ApplicationInfo
    {
        public static void GetExeInfo()
        {
            // Get application data to later use in tool and log
            AssemblyCopyrightAttribute copyright = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0] as AssemblyCopyrightAttribute;
            // ReSharper disable once PossibleNullReferenceException
            Globals._copyrightData = copyright.Copyright;

            // Get application data to later use in tool and log
            Globals._vData = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
            var attributes = typeof(Program).GetTypeInfo().Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute));
            var assemblyTitleAttribute = attributes.SingleOrDefault() as AssemblyTitleAttribute;

            // Set application name in code and log
            // Get the base application name from assemblyTitleAttribute?.Title
            string baseAppName = assemblyTitleAttribute?.Title;

            // Check if the base application name contains "Tool"
            if (baseAppName != null && baseAppName.Contains("Tool"))
            {
                // Remove "Tool" from the application name
                baseAppName = baseAppName.Replace("Tool", string.Empty).Trim();
            }

            // Set the modified application name
            Globals.AppName = baseAppName;

            // Globals.AppName = assemblyTitleAttribute?.Title;

            // Set exe file name in code and log
            Globals._currentExeFileName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName);

            // Set company name in code and log
            var fileName = Assembly.GetEntryAssembly()?.Location;
            if (fileName != null)
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(fileName);
                Globals._companyName = versionInfo.CompanyName;
            }
        }
    }
}
