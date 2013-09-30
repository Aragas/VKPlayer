using System;

namespace PluginVK
{
    /// <summary>
    /// Contains various constants used by project.
	/// </summary>
	internal static class Constants
	{
		#region Content
        public static string data_name = "DataAudio.tmp";
        public static string dir = Environment.GetEnvironmentVariable("TEMP") + @"\";
        //public static string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Rainmeter\Plugins\VKPlugin\";
        public static string path_data = dir + data_name;
		#endregion

	}
}
