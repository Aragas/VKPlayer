using Rainmeter.AudioPlayer;
using Rainmeter.Forms;

namespace Rainmeter.Plugin
{
    public static class Verification
    {
        private static bool TokenIdExists
        {
            get
            {
                return (OAuth.Token != null || OAuth.Id != null);
            }
        }

        public static void StartExecute(string command)
        {
            if (!TokenIdExists)
            {
#if DEBUG
                OAuth.OAuthRun();
                Player.Execute(command, OAuth.Token, OAuth.Id);
#else
                try
                {
                    OAuth.OAuthRun();
                    Player.Execute(command, OAuth.Token, OAuth.Id);
                }
                catch { }
#endif
            }
            else
            {
#if DEBUG
                Player.Execute(command, OAuth.Token, OAuth.Id);
#else
                try
                {
                    Player.Execute(command, OAuth.Token, OAuth.Id);
                }
                catch { }
#endif
            }
        }
    }
}