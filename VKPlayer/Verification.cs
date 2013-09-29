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

using PlayerVK;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace PluginVK
{
    public static class Verification
    {
        private static string crypted_id = null;
        public static string Id = null;
        public static string Token = null;

        public static void Audio(string Command)
        {
            if (Id == null || Token == null)
            {
                #region Dir
                // Проверка файла на существование.
                if (!Directory.Exists(Constants.dir))
                {
                    Directory.CreateDirectory(Constants.dir);
                }
                if (!File.Exists(Constants.path_data))
                {
                    using (FileStream stream = File.Create(Constants.path_data)) { }
                }
                #endregion

                // Чтение параметров.
                using (StreamReader sr = new StreamReader(Constants.path_data, Encoding.UTF8))
                {
                    crypted_id = sr.ReadLine();

                    if (crypted_id != null)
                    {
                        Crypto cr = new Crypto();
                        Id = cr.Decrypt(crypted_id, "ididitjustforlulz");
                        Token = cr.Decrypt(sr.ReadLine(), "ididitjustforlulz");
                    }
                }

                // Проверка существования данных.
                if (crypted_id == null)
                {
                    OAuth.OAuthRun();
                }
                else
                {
                    GetAudio.Get(Command, Token, Id);
                }
            }
            else
            {
                GetAudio.Get(Command, Token, Id);
            }
  

        }

        public static void Online()
        {
            if (Id == null || Token == null)
            {
                #region Dir
                // Проверка файла на существование.
                if (!Directory.Exists(Constants.dir))
                {
                    Directory.CreateDirectory(Constants.dir);
                }
                if (!File.Exists(Constants.path_data))
                {
                    using (FileStream stream = File.Create(Constants.path_data)) { }
                }
                #endregion

                // Чтение параметров.
                using (StreamReader sr = new StreamReader(Constants.path_data, Encoding.UTF8))
                {
                    crypted_id = sr.ReadLine();

                    if (crypted_id != null)
                    {
                        Crypto cr = new Crypto();
                        Id = cr.Decrypt(crypted_id, "ididitjustforlulz");
                        Token = cr.Decrypt(sr.ReadLine(), "ididitjustforlulz");
                    }
                }

                // Проверка существования данных.
                if (crypted_id == null)
                {
                    OAuth.OAuthRun();
                    Get.GetOnlineMessage(Token, Id);
                }
                else
                {
                    Get.GetOnlineMessage(Token, Id);
                }
            }

        }
    }
}

