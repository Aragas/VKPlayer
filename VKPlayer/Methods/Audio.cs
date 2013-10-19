using System;
using System.Xml;

namespace Rainmeter.Methods
{
    /// <summary>
    /// Operations with audio.
    /// </summary>
    public class Audio
    {
        /// <summary>
        /// Set your Token.
        /// </summary>
        public string Token { private get; set; }

        /// <summary>
        /// Set your Id.
        /// </summary>
        public string Id { private get; set; }

        private string AudioCount()
        {
            // Parameters.
            const string method = "audio.getCount.xml?";
            var param = "owner_id=" + Id;

            // Getting document.
            var doc = new XmlDocument();
            doc.Load("https://api.vk.com/method/" + method + param + "&access_token=" + Token);

            #region ErrorCheck

            XmlNode root = doc.DocumentElement;
            XmlNodeList nodeListError = root.SelectNodes("error_code");

            const string checkerror = "<error_code>5</error_code>";
            const string checkerror2 = "<error_code>7</error_code>";

            foreach (XmlNode node in nodeListError)
            {
                if (node.OuterXml.Equals(checkerror) || node.OuterXml.Equals(checkerror2)) return null;
            }

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
            // Parameters.
            const string method = "audio.get.xml?";
            var param = "owner_id=" + Id + "&count=" + AudioCount();

            // Getting document.
            var doc = new XmlDocument();
            doc.Load("https://api.vk.com/method/" + method + param + "&access_token=" + Token);

            #region ErrorCheck

            XmlNode root = doc.DocumentElement;
            XmlNodeList nodeListError = root.SelectNodes("error_code");

            const string checkerror = "<error_code>5</error_code>";
            const string checkerror2 = "<error_code>7</error_code>";

            foreach (XmlNode node in nodeListError)
            {
                if (node.OuterXml.Equals(checkerror) || node.OuterXml.Equals(checkerror2)) return null;
            }

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