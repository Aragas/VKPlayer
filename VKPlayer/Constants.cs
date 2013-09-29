using System;

namespace PluginVK
{
    /// <summary>
    /// Contains various constants used by project.
	/// </summary>
	internal static class Constants
	{
		#region Content
        public static string data_name = "Data.tmp";
        public static string onlineusers_name = "OnlineUsers.tmp";
        public static string audio = "Audio.tmp";
        //public static string dir = Environment.GetEnvironmentVariable("TEMP") + "\\";
        public static string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Rainmeter\Plugins\VKPlugin\";
        public static string path_data = dir + data_name;
        public static string path_onlineusers = dir + onlineusers_name;
        public static string path_audio = dir + audio;
        public static int count = 5;
		#endregion

	}
}
