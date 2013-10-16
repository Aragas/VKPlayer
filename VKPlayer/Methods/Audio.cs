using System;
using System.Xml;

namespace Rainmeter.Methods
{
    /// <summary>
    ///     Operations with audio.
    /// </summary>
    public class Audio
    {
        public string Token { get; set; }
        public string Id { get; set; }

        private string AudioCount()
        {
            // Параметры конфигурации.
            const string method = "audio.getCount.xml?";
            var param = "owner_id=" + Id;

            //Получение документа.
            var doc = new XmlDocument();
            doc.Load("https://api.vk.com/method/" + method + param + "&access_token=" + Token);

            XmlNode root = doc.DocumentElement;

            #region ErrorCheck

            XmlNodeList nodeListError = root.SelectNodes("error_code");
            // Выявление ошибочного запрса.
            var sucheck = "";
            const string sucheckerror = "<error_code>5</error_code>";
            const string sucheckerror2 = "<error_code>7</error_code>";

            foreach (XmlNode node in nodeListError)
            {
                sucheck = node.OuterXml;
            }

            if (sucheck == sucheckerror) return null;
            if (sucheck == sucheckerror2) return null;

            #endregion

            var countstring = "0";
            try
            {
                countstring = root["response"].InnerText;
            }
            catch {}

            return countstring;
        }

        public string[] AudioList()
        {
            var arr3 = new string[0];
            // Параметры конфигурации.
            const string method = "audio.get.xml?";
            var param = "owner_id=" + Id + "&count=" + AudioCount();

            //Получение документа.
            var doc = new XmlDocument();
            doc.Load("https://api.vk.com/method/" + method + param + "&access_token=" + Token);

            XmlNode root = doc.DocumentElement;

            #region ErrorCheck

            XmlNodeList nodeListError = root.SelectNodes("error_code");

            // Выявление ошибочного запрса.
            var sucheck = "";
            const string sucheckerror = "<error_code>5</error_code>";
            const string sucheckerror2 = "<error_code>7</error_code>";

            foreach (XmlNode node in nodeListError)
            {
                sucheck = node.OuterXml;
            }

            if (sucheck == sucheckerror) return null;
            if (sucheck == sucheckerror2) return null;

            #endregion

            #region Filtering

            foreach (XmlNode node in doc.SelectNodes("//audio"))
            {
                const string space = "#";
                var artist = node["artist"].InnerText;
                var title = node["title"].InnerText;
                var duration = node["duration"].InnerText;
                var url = node["url"].InnerText.Split('?')[0];

                if (artist.Contains("&amp;")) artist = artist.Replace("&amp;", "&");
                if (title.Contains("&amp;")) title = title.Replace("&amp;", "&");

                Array.Resize(ref arr3, arr3.Length + 1);
                arr3[arr3.Length - 1] = space + artist + space + title + space + duration + space + url;
            }

            return arr3;

            #endregion
        }
    }
}