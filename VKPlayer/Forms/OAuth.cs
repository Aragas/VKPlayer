using System;
using System.Windows.Forms;

namespace Rainmeter.Forms
{
    public partial class OAuth : Form
    {
        public static string Token;
        public static string Id;

        public OAuth()
        {
            InitializeComponent();

            const string url = "https://oauth.vk.com/authorize?client_id=3328403"
                               + "&redirect_uri=https://oauth.vk.com/blank.html"
                               + "&scope=audio&display=popup&response_type=token";
            webBrowser1.Navigate(url);
        }

        public static void OAuthRun()
        {
            Application.Run(new OAuth());
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var tokenid = webBrowser1.Url.ToString().Split('#')[1];

            Token = tokenid.Split('&')[0].Split('=')[1];
            Id = tokenid.Split('=')[3];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}