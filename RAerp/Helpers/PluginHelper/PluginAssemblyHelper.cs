using RAerp.PluginServiceProvider;
using System.Reflection;

namespace RAerp.Helpers.PluginHelper
{
    public static class PluginAssemblyHelper
    {
        private static readonly List<string> corePlugins = new List<string>() {
            "RA.EntityTypes",
            "RA.FormTypes",
            "RA.WebServiceEndpoints"
        };
        public static List<Assembly> GetAllPluginAssemblies()
        {
            var assemblyList = new List<Assembly>();

            // Get the path to the folder you're interested in (e.g. "MyFolder")
            // not working in somee.com - free hosting
            //var path = Path.GetFullPath("D:\\RitzAlonzoSystem\\RAerp\\Plugins");
            // get the directory of the saved project
            // try in somee.com
            var currentDirectory = Directory.GetCurrentDirectory();
            currentDirectory = currentDirectory + "\\Plugins";
            // Get all the folders in Plugins
            var pluginFolders = Directory.GetDirectories(currentDirectory);

            foreach (var pluginFolder in pluginFolders)
            {
                var pluginFolderName = pluginFolder.Substring(pluginFolder.LastIndexOf("RA"));

                // Get all the DLL files in the folder
                var pluginDll = Directory.GetFiles(pluginFolder, $"{pluginFolderName}.dll").FirstOrDefault();

                if (pluginDll != null)
                {
                    // Load the assembly
                    var assembly = Assembly.LoadFrom(pluginDll);

                    if (assembly != null)
                        assemblyList.Add(assembly);
                }
            }

            return assemblyList;
        }

        public static List<Assembly> GetAllCorePluginAssemblies()
        {
            var assemblyList = new List<Assembly>();

            // Get the path to the folder you're interested in (e.g. "MyFolder")
            //var path = Path.GetFullPath("D:\\RitzAlonzoSystem\\RAerp\\Plugins");
            // get the directory of the saved project
            // try in somee.com
            var currentDirectory = Directory.GetCurrentDirectory();
            currentDirectory = currentDirectory + "\\Plugins";
            // Get all the folders in Plugins
            var pluginFolders = Directory.GetDirectories(currentDirectory)
                                .Where(c => corePlugins.Any(x => x.EndsWith(c.Substring(c.LastIndexOf("RA")))));

            foreach (var pluginFolder in pluginFolders)
            {
                var pluginFolderName = pluginFolder.Substring(pluginFolder.LastIndexOf("RA"));

                // Get all the DLL files in the folder
                var pluginDll = Directory.GetFiles(pluginFolder, $"{pluginFolderName}.dll").FirstOrDefault();

                if (pluginDll != null)
                {
                    // Load the assembly
                    var assembly = Assembly.LoadFrom(pluginDll);

                    if (assembly != null)
                        assemblyList.Add(assembly);
                }
            }

            return assemblyList;
        }

        public static List<Assembly> GetAllModulesPluginAssemblies()
        {
            var assemblyList = new List<Assembly>();

            // Get the path to the folder you're interested in (e.g. "MyFolder")
            //var path = Path.GetFullPath("D:\\RitzAlonzoSystem\\RAerp\\Plugins");
            // get the directory of the saved project
            // try in somee.com
            var currentDirectory = Directory.GetCurrentDirectory();
            currentDirectory = currentDirectory + "\\Plugins";

            // Get all the folders in Plugins
            var pluginFolders = Directory.GetDirectories(currentDirectory)
                                .Where(c => !corePlugins.Any(x => x.EndsWith(c.Substring(c.LastIndexOf("RA")))));

            foreach (var pluginFolder in pluginFolders)
            {
                var pluginFolderName = pluginFolder.Substring(pluginFolder.LastIndexOf("RA"));

                // Get all the DLL files in the folder
                var pluginDll = Directory.GetFiles(pluginFolder, $"{pluginFolderName}.dll").FirstOrDefault();

                if (pluginDll != null)
                {
                    // Load the assembly
                    var assembly = Assembly.LoadFrom(pluginDll);

                    if (assembly != null)
                        assemblyList.Add(assembly);
                }
            }

            return assemblyList;
        }

        public static Assembly GetPluginAssembly(string pluginName)
        {
            Assembly pluginAssembly = null;
            // Get the path to the folder you're interested in (e.g. "MyFolder")
            //var path = Path.GetFullPath("D:\\RitzAlonzoSystem\\RAerp\\Plugins");
            // get the directory of the saved project
            // try in somee.com
            var currentDirectory = Directory.GetCurrentDirectory();
            currentDirectory = currentDirectory + "\\Plugins";

            // Get the folder in Plugins
            var pluginFolder = Directory.GetDirectories(currentDirectory)
                                .Where(c => c.Contains(pluginName, StringComparison.InvariantCultureIgnoreCase))
                                .FirstOrDefault();

            if (pluginFolder != null )
            {
                var pluginFolderName = pluginFolder.Substring(pluginFolder.LastIndexOf("RA"));

                var pluginDll = Directory.GetFiles(pluginFolder, $"{pluginFolderName}.dll").FirstOrDefault();

                if (pluginDll != null)
                {
                    // Load the assembly
                    pluginAssembly = Assembly.LoadFrom(pluginDll);
                }

            }

            return pluginAssembly;
        }
    }
}
