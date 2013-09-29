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

using System.IO;
using System.Threading;

namespace PluginVK
{
    class Get
    {
        public static void GetOnlineMessage(string token, string id)
        {
            string text1 = null;
            string text2 = null;
            string text3 = null;

            #region Friends
            Thread t1 = new Thread(delegate() 
                {
                    Friends fr = new Friends();
                    fr.token = token;
                    fr.id = id;
                    fr.path = Constants.path_onlineusers;
                    fr.count = Constants.count;
                    text1 = fr.ConvertFriendsOnline();
                });
            t1.Start();
            #endregion

            #region Messages
            Thread t2 = new Thread(delegate()
            {
                Messages ms = new Messages();
                ms.token = token;
                ms.id = id;
                text2 = ms.UnReadedMessages();
            });
            t2.Start();
            #endregion

            t1.Join();
            t2.Join();

            string text = text1 + text2 + "&";

            if (text1 == null || text2 == null)
            {
                OAuth.OAuthRun();
            }

            using (StreamWriter outfile = new StreamWriter(Constants.path_onlineusers))
            {
                outfile.Write(text);
            }
        }
    }
}
