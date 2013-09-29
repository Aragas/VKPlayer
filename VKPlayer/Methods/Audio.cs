using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace PluginVK
{
    public class Audio
    {
        public string token { get; set; }
        public string id { get; set; }

        private string AudioCount()
        {
            // Параметры конфигурации.
            string method = "audio.getCount.xml?";
            string param = "owner_id=" + id;

            //Получение документа.
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
                countstring = root["response"].InnerText;
            }
            catch { }

            return countstring;
        }

        public string[] AudioList()
        {
            string[] arr3 = new string[0];
            // Параметры конфигурации.
            string method = "audio.get.xml?";
            string param = "owner_id=" + id + "&count=" + AudioCount();

            //Получение документа.
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

            #region Filtering
            using (Stream onlinems = new MemoryStream())
            {

                foreach (XmlNode node in doc.SelectNodes("//audio"))
                {
                    string space = "#";
                    string artist = node["artist"].InnerText;
                    string title = node["title"].InnerText;
                    string duration = node["duration"].InnerText;
                    string url = node["url"].InnerText.Split('?')[0];

                    Array.Resize(ref arr3, arr3.Length + 1);
                    arr3[arr3.Length - 1] = space + artist + space + title + space + duration + space + url;
                    //string[] array = { space + artist + space + title + duration + space + url };
                    //array.CopyTo(arr3, array.Length);

                    #region
                    //byte[] ubytes1 = Encoding.UTF8.GetBytes(artist);
                    //byte[] ubytes2 = Encoding.UTF8.GetBytes(title);
                    //byte[] ubytes3 = Encoding.UTF8.GetBytes(duration);
                    //byte[] ubytes4 = Encoding.UTF8.GetBytes(url);
                    //byte[] ubytes5 = Encoding.UTF8.GetBytes(space);

                    //onlinems.Write(ubytes5, 0, ubytes5.Length);
                    //onlinems.Write(ubytes1, 0, ubytes1.Length);
                    //onlinems.Write(ubytes5, 0, ubytes5.Length);
                    //onlinems.Write(ubytes2, 0, ubytes2.Length);
                    //onlinems.Write(ubytes5, 0, ubytes5.Length);
                    //onlinems.Write(ubytes3, 0, ubytes3.Length);
                    //onlinems.Write(ubytes5, 0, ubytes5.Length);
                    //onlinems.Write(ubytes4, 0, ubytes4.Length);
                    #endregion
                }

                // Добавление в конец последней &.
                //string space1 = "&&";
                //byte[] ubytes51 = Encoding.UTF8.GetBytes(space1);
                //onlinems.Write(ubytes51, 0, ubytes51.Length);

                //using (StreamReader reader = new StreamReader(onlinems))
                //{
                //    onlinems.Position = 0;
                //    text = reader.ReadToEnd();
                //}

                //return text;
                return arr3;
            }
            #endregion
        }
    }
}
