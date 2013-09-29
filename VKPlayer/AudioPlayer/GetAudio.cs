namespace PlayerVK
{
    static class GetAudio
    {
        public static void Get(string Command, string token, string id)
        {
            Player.Token = token;
            Player.Id = id;

            if (Command == "PlayPause")
            {
                Player.PlayPause();
            }

            else if (Command == "Stop")
            {
                Player.Stop();
            }

            else if (Command == "Next")
            {
                Player.Next();
            }

            else if (Command == "Previous")
            {
                Player.Previous();
            }

            else
            {
            }
        }
    }
}
