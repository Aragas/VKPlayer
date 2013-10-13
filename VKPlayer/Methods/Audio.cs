using System;
using System.IO;
using System.Xml;

namespace VKPlayer
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
            nodeListError = root.SelectNodes("error_code");
            // Выявление ошибочного запрса.
            string sucheck = "";
            string sucheckerror = "<error_code>5</error_code>";
            string sucheckerror2 = "<error_code>7</error_code>";

            foreach (XmlNode node in nodeListError)
            {
                sucheck = node.OuterXml;
            }

            if (sucheck == sucheckerror)  return null;
            if (sucheck == sucheckerror2) return null;

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

            if (sucheck == sucheckerror)  return null;
            if (sucheck == sucheckerror2) return null;
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

                    if (artist.Contains("&amp;")) artist = artist.Replace("&amp;", "&");
                    if (title.Contains("&amp;")) title = title.Replace("&amp;", "&");

                    Array.Resize(ref arr3, arr3.Length + 1);
                    arr3[arr3.Length - 1] = space + artist + space + title + space + duration + space + url;
                }

                return arr3;
            }
            #endregion
        }
    }
}
