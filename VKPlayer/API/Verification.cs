using System;
using System.IO;
using System.Net;
using System.Text;

namespace VKPlayer
{
    public static class Verification
    {
        public static bool TokenIdExists
        {
            get
            {
                if (OAuth.Token != null || OAuth.Id != null) return true;
                else return false;
            }
        }

        public static void StartExecute(string Command)
        {
            if (!TokenIdExists)
            {
#if DEBUG
                OAuth.OAuthRun();
                Player.Execute(Command, OAuth.Token, OAuth.Id);
#else
                    try
                {
                    OAuth.OAuthRun();
                    Player.Execute(Command, OAuth.Token, OAuth.Id);
                }
                catch { }
#endif
            }
            else
            {
#if DEBUG
                Player.Execute(Command, OAuth.Token, OAuth.Id);
#else
                try
                {
                    Player.Execute(Command, OAuth.Token, OAuth.Id);
                }
                catch { }
#endif
            }
        }

    }
}

