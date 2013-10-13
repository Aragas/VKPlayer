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
                try
                {
                    OAuth.OAuthRun();
                    Player.Execute(Command, Token, Id);
                }
                catch { }
            }
            else
            {
                try
                {
                    Player.Execute(Command, Token, Id);
                }
                catch { }
            }
        }

    }
}

