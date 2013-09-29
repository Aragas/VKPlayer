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
using System.IO;
using System.Text;
using System.Xml;

namespace PluginVK
{
    public class Friends
    {
        public string token { get; set; }
        public string id { get; set;  }
        public int count { get; set; }
        public string path { get; set; }

        private string _document { get { return document(token, id, count); } }
        private string document(string token, string id, int count)
        {
            // Первичные значения (Из-за using).
            string DocumentString = null;

            // Параметры конфигурации.
            string method = "friends.get.xml?";
            string param = "uid=" + id + "&order=hints" + "&fields=first_name,last_name,photo_50,online";

            //Получение документа.
            XmlDocument doc = new XmlDocument();
            doc.Load("https://api.vk.com/method/" + method + param + "&access_token=" + token);

            XmlNode root = doc.DocumentElement;

            #region ErrorCheck
            // Создание Node списков.
            XmlNodeList nodeList;
            XmlNodeList nodeListError;

            nodeList = root.SelectNodes("/response/user[online='1']"); // Параметр, по которому идет фильтрация.
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

            #region Filtering
            // Фильтрация документа по параметру.
            using (Stream DocumentStream = new MemoryStream())
            {
                // Запись главной ветки (Нужно для Xml парсера).
                string r1 = "<main>";
                byte[] rb1 = Encoding.UTF8.GetBytes(r1);
                DocumentStream.Write(rb1, 0, rb1.Length);

                int x = 0;
                foreach (XmlNode node in nodeList)
                {
                    string susers = node.OuterXml;
                    byte[] ubytes = Encoding.UTF8.GetBytes(susers);
                    DocumentStream.Write(ubytes, 0, ubytes.Length);

                    x = x + 1;
                    if (x == count) break;
                }

                // Запись главной ветки (Нужно для Xml парсера).
                string r2 = "</main>";
                byte[] rb2 = Encoding.UTF8.GetBytes(r2);
                DocumentStream.Write(rb2, 0, rb2.Length);

                // Конвертирование MemoryStream в байты.
                Byte[] byteArray = new byte[DocumentStream.Length];
                DocumentStream.Position = 0;
                DocumentStream.Read(byteArray, 0, (int)DocumentStream.Length);

                // Конвертирование байтов в string.
                DocumentString = Encoding.UTF8.GetString(byteArray);
            }
            #endregion

            return DocumentString;
        }

        public string ConvertFriendsOnline()
        {
            if (_document == null)
            {
                return null;
            }

            string text = "";
            // Создание потока и запись в него 
            // последующих пользователей онлайн.
            using (Stream onlinems = new MemoryStream())
            {
                XmlDocument doc0 = new XmlDocument();
                doc0.LoadXml(_document);

                // Запись параметров пользователей.
                foreach (XmlNode node in doc0.SelectNodes("//user"))
                {
                    string space = "&";
                    string uids = node["uid"].InnerText;
                    string first = node["first_name"].InnerText;
                    string last = node["last_name"].InnerText;
                    string photo = node["photo_50"].InnerText;
                    string online_m = "";

                    // Проверка существования <online_mobile>.
                    if (node.SelectSingleNode("online_mobile") == null)
                    {
                        online_m = "0";
                    }

                    else
                    {
                        online_m = node["online_mobile"].InnerText;
                    }

                    byte[] ubytes1 = Encoding.UTF8.GetBytes(uids);
                    byte[] ubytes2 = Encoding.UTF8.GetBytes(first);
                    byte[] ubytes3 = Encoding.UTF8.GetBytes(last);
                    byte[] ubytes4 = Encoding.UTF8.GetBytes(photo);
                    byte[] ubytes5 = Encoding.UTF8.GetBytes(online_m);
                    byte[] ubytes6 = Encoding.UTF8.GetBytes(space);

                    onlinems.Write(ubytes6, 0, ubytes6.Length);
                    onlinems.Write(ubytes1, 0, ubytes1.Length);
                    onlinems.Write(ubytes6, 0, ubytes6.Length);
                    onlinems.Write(ubytes2, 0, ubytes2.Length);
                    onlinems.Write(ubytes6, 0, ubytes6.Length);
                    onlinems.Write(ubytes3, 0, ubytes3.Length);
                    onlinems.Write(ubytes6, 0, ubytes6.Length);
                    onlinems.Write(ubytes4, 0, ubytes4.Length);
                    onlinems.Write(ubytes6, 0, ubytes6.Length);
                    onlinems.Write(ubytes5, 0, ubytes5.Length);
                }

                // Добавление в конец последней &.
                string space1 = "&";
                byte[] ubytes51 = Encoding.UTF8.GetBytes(space1);
                onlinems.Write(ubytes51, 0, ubytes51.Length);

                using (StreamReader reader = new StreamReader(onlinems))
                {
                    onlinems.Position = 0;
                    text = reader.ReadToEnd();
                }

                return text;
            }
        }
    }
}
