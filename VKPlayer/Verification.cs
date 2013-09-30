using PlayerVK;
using System;
using System.IO;
using System.Text;

namespace PluginVK
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
        public static bool DirExists
        {
            get
            {
                if (!Directory.Exists(Constants.dir)) return true;
                else return false;
            }
        }
        public static bool FileExists
        {
            get
            {
                if (File.Exists(Constants.path_data)) return true;
                else return false;
            }
        }

        private static string crypted_id = null;

        public static void Start(string Command)
        {
            if (!TokenIdExists)
            {
                if (!DirExists) DirCreate();
                if (!FileExists) FileCreate();

                // Чтение параметров.
                using (StreamReader sr = new StreamReader(Constants.path_data, Encoding.UTF8))
                {
                    crypted_id = sr.ReadLine();

                    if (crypted_id != null)
                    {
                        Crypto cr = new Crypto();
                        Id = cr.Decrypt(crypted_id, "ididitjustforlulz");
                        Token = cr.Decrypt(sr.ReadLine(), "ididitjustforlulz");

                        GetAudio.Get(Command, Token, Id);
                    }
                    else OAuth.OAuthRun();
                }

            }
            else GetAudio.Get(Command, Token, Id);
        }

        public static void DirCreate()
        {
            Directory.CreateDirectory(Constants.dir);
        }

        public static void FileCreate()
        {
            using (FileStream fs = File.Create(Constants.path_data)) { }
        }
    }
}

