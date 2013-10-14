using Rainmeter.AudioPlayer;
using Rainmeter.Forms;

namespace Rainmeter.API
{
    public static class Verification
    {
        public static bool TokenIdExists
        {
            get
            {
                if (OAuth.Token != null || OAuth.Id != null) return true;
                return false;
            }
        }

        public static void StartExecute(string сommand)
        {
            if (!TokenIdExists)
            {
#if DEBUG
                OAuth.OAuthRun();
                Player.Execute(сommand, OAuth.Token, OAuth.Id);
#else
                    try
                {
                    OAuth.OAuthRun();
                    Player.Execute(сommand, OAuth.Token, OAuth.Id);
                }
                catch { }
#endif
            }
            else
            {
#if DEBUG
                Player.Execute(сommand, OAuth.Token, OAuth.Id);
#else
                try
                {
                    Player.Execute(сommand, OAuth.Token, OAuth.Id);
                }
                catch { }
#endif
            }
        }
    }
}