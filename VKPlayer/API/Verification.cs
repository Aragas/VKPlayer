using Rainmeter.AudioPlayer;
using Rainmeter.Forms;

namespace Rainmeter.API
{
    public static class Verification
    {
        private static bool TokenIdExists
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
                Execute(сommand, OAuth.Token, OAuth.Id);
#else
                    try
                {
                    OAuth.OAuthRun();
                    Execute(сommand, OAuth.Token, OAuth.Id);
                }
                catch { }
#endif
            }
            else
            {
#if DEBUG
                Execute(сommand, OAuth.Token, OAuth.Id);
#else
                try
                {
                    Execute(сommand, OAuth.Token, OAuth.Id);
                }
                catch { }
#endif
            }
        }

        private static void Execute(string command, string token, string id)
        {
            Player.Token = token;
            Player.Id = id;

            if (command == "PlayPause") PlayPause();
            else if (command == "Play") PlayPause();
            else if (command == "Pause") PlayPause();
            else if (command == "Stop") Stop();
            else if (command == "Next") Next();
            else if (command == "Previous") Previous();
            else if (command.Contains("SetVolume")) SetVolume(command.Remove(0, 10));
            else if (command.Contains("SetShuffle")) SetShuffle(command.Remove(0, 11));
            else if (command.Contains("SetRepeat")) SetRepeat(command.Remove(0, 10));
            else if (command.Contains("SetPosition")) SetPosition(command.Remove(0, 12));
            else if (command.Contains("SetRating")) SetRating(command.Remove(0, 10));
            else return;
        }
    }
}