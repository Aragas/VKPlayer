using System;
using System.IO;
using System.Net;
using System.Text;

namespace VKPlayer
{
    public static class Verification
    {
        public static string Id;
        public static string Token;

        public static bool TokenIdExists
        {
            get
            {
                if (Token != null || Id != null) return true;
                else return false;
            }
        }

        public static void StartExecute(string Command)
        {
            if (!TokenIdExists)
            {
                //try
                //{
                    OAuth.OAuthRun();
                    Player.Execute(Command, Token, Id);
                //}
                //catch { }
            }
            else
            {
                //try
                //{
                    Player.Execute(Command, Token, Id);
                //}
                //catch { }
            }
        }

        static void Check()
        {
            string url = "http://oauth.vk.com/authorize?client_id=3328403"
                + "&redirect_uri=https://oauth.vk.com/blank.html"
                + "&scope=audio&display=popup&response_type=token";

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "GET";
            //httpWebRequest.AllowAutoRedirect = true;
            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                if (httpWebResponse.GetResponseHeader("Content-Length") == "6254") ;
                {
                }
                Console.Write(httpWebResponse.GetResponseHeader("Content-Length"));
            }
            Console.Write("{0}Press any key to continue...", Environment.NewLine);
            Console.ReadKey();
        }

    }
}

