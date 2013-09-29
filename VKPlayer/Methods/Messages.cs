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
using System.Xml;

namespace PluginVK
{
    public class Messages
    {
        public string token { get; set; }
        public string id { get; set; }

        public string UnReadedMessages()
        {
            // Параметры.
            string method = "messages.get.xml?";
            string param = "&filters=1";

            XmlDocument doc = new XmlDocument();
            doc.Load("https://api.vk.com/method/" + method + param + "&access_token=" + token);

            XmlNode root = doc.DocumentElement;

            #region ErrorCheck
            XmlNodeList nodeListError;
            nodeListError = root.SelectNodes("error_code"); // Для выявления ошибочного запроса.

            // Выявление ошибочного запрса.
            string sucheck = "";
            string sucheckerror = "<error_code>5</error_code>";
            string sucheckerror2 = "<error_code>7</error_code>";

            foreach (XmlNode node in nodeListError)
            {
                sucheck = node.OuterXml;
            }

            if (sucheck == sucheckerror)
            {
                return null;
            }

            if (sucheck == sucheckerror2)
            {
                return null;
            }
            #endregion

            string countstring = "0";

            try
            {
                countstring = root["count"].InnerText;
            }
            catch { }

            int i = Convert.ToInt32(countstring);
            if (i > 1)
            {
                i = 1;
            }

            countstring = Convert.ToString(i);

            return countstring;
        }

    }
}
