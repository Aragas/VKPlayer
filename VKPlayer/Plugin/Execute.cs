using Rainmeter.AudioPlayer;
using Rainmeter.Forms;

namespace Rainmeter.Plugin
{
    public static class Execute
    {
        private static bool TokenIdExists
        {
            get { return (OAuth.Token != null || OAuth.Id != null); }
        }

        public static void Start(string command)
        {
#if DEBUG
            if (!TokenIdExists)
            {
                OAuth.OAuthRun();
                Player.Execute(command, OAuth.Token, OAuth.Id);
            }
            else
            {
                Player.Execute(command, OAuth.Token, OAuth.Id);
            }
#else
            if (!TokenIdExists)
            {
                try
                {
                    OAuth.OAuthRun();
                    Player.Execute(command, OAuth.Token, OAuth.Id);
                }
                catch {}
            }
            else
            {
                try
                {
                    Player.Execute(command, OAuth.Token, OAuth.Id);
                }
                catch {}
            }
#endif
        }
    }
}