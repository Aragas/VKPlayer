using System;
using System.Windows.Forms;

namespace Rainmeter.Forms
{
    /// <summary>
    ///     Get Token and Id.
    /// </summary>
    public partial class OAuth : Form
    {
        /// <summary>
        ///     Get your Token (Use after OAuthRun()).
        /// </summary>
        public static string Token;

        /// <summary>
        ///     Get your Id (Use after OAuthRun()).
        /// </summary>
        public static string Id;

        private OAuth()
        {
            InitializeComponent();
            webBrowser1.Navigate(Url);
        }

        private static string Url
        {
            get
            {
                return "https://oauth.vk.com/authorize?client_id=3328403"
                       + "&redirect_uri=https://oauth.vk.com/blank.html"
                       + "&scope=audio&display=popup&response_type=token"
#if DEBUG
                       + "&revoke=1";
#else
                       + "&revoke=0";
#endif
            }
        }

        /// <summary>
        ///     Run Form.
        /// </summary>
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
            string data = webBrowser1.Url.ToString().Split('#')[1];

            Token = data.Split('&')[0].Split('=')[1];
            Id = data.Split('=')[3];

            Close();
        }
    }
}