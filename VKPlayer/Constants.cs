/*
  Copyright (C) 2013 Aragas (Aragasas)

  This program is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

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
