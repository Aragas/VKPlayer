using System;
using System.Windows.Forms;

namespace Rainmeter.Forms
{
    public partial class OAuth : Form
    {
        public static string Token;
        public static string Id;

        private static string Url
        {
            get
            {
                return "https://oauth.vk.com/authorize?client_id=3328403"
                       + "&redirect_uri=https://oauth.vk.com/blank.html"
                       + "&scope=audio&display=popup&response_type=token" +
#if DEBUG
                    "&revoke=1";
#else
                                "";
#endif
            }
        }

        public OAuth()
        {
            InitializeComponent();
            webBrowser1.Navigate(Url);
        }

        public static void OAuthRun()
        {
            Application.Run(new OAuth());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser1.Url.ToString() == Url)
            {
                WindowState = FormWindowState.Normal;
            }
            else SaveData();
        }

        private void SaveData()
        {
            var tokenid = webBrowser1.Url.ToString().Split('#')[1];

            Token = tokenid.Split('&')[0].Split('=')[1];
            Id = tokenid.Split('=')[3];

            Close();
        }

    }
}