using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NAudio.Wave;
using PluginVK;

namespace PlayerVK
{
    public class Player
    {
        public enum Playing
        {
            Init,
            Play,
            Pause,
            Stop
        }
        public static Playing option = Playing.Init;

        static GetStream gStream = new GetStream();
        static WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
        static Audio au = new Audio();

        public static string Token;
        public static string Id;

        public static int time = 0;
        private static string[] _array;
        private static string[] array
        {
            get
            {
                if (_array == null)
                {
                    au.token = Token;
                    au.id = Id;
                    _array = au.AudioList();
                    return _array;
                }
                else
                {
                    return _array;
                }
            }
            set { }
        }
        private static int numb = 0;
        private static string url
        {
            get
            {
                return array[numb].Split('#')[4];
            }
            set { }
        }

        #region GetString()
        public static string Artist
        {
            get
            {
                if (array == null)
                {
                    return null;
                }
                else
                {
                    return array[numb].Split('#')[1];
                }
            }
            set { }
        }
        public static string Title
        {
            get
            {
                if (array == null)
                {
                    return null;
                }
                else
                {
                    return array[numb].Split('#')[2];
                }
            }
            set { }
        }
        public static int Duration
        {
            get
            {
                if (array == null)
                {
                    return 0;
                }
                else
                {
                    return Convert.ToInt32(array[numb].Split('#')[3]);
                }
            }
            set { }
        }
        #endregion

        #region ExecuteBang()
        public static void PlayPause()
        {
            if (option == Playing.Play)
            {
                Pause();  
            }
            else if (option == Playing.Pause)
            {
                playPause();
            }
            else if (option == Playing.Init)
            {
                ThreadStart work = delegate
                {
                    try
                    {
                        Play();
                    }
                    catch { }
                };
                new Thread(work).Start();
            }
            else if (option == Playing.Stop)
            {
                Play();
            }
        }
        private static void Play()
        {
            option = Playing.Play;

            gStream.url = url;

            waveOut.Init(gStream.Wave());
            waveOut.Play();

        }
        private static void playPause()
        {
            waveOut.Play();
        }

        public static void Stop()
        {
            if (option != Playing.Stop)
            {
                option = Playing.Stop;

                waveOut.Stop();
            }
        }
        public static void Pause()
        {
            if (option == Playing.Play)
            {
                option = Playing.Pause;

                waveOut.Pause();
            }
        }
        public static void Next()
        {
            if (option != Playing.Stop)
            {
                if (numb < array.Length)
                {
                    numb += 1;

                    Stop();
                    waveOut.Dispose();
                    waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
                    gStream.Dispose();
                    gStream = new GetStream();
                    Play();
                }
            }
        }
        public static void Previous()
        {
            if (option != Playing.Stop)
            {

                if (numb > 0)
                {
                    numb -= 1;;

                    Stop();
                    waveOut.Dispose();
                    waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
                    gStream.Dispose();
                    gStream = new GetStream();
                    Play();
                }
            }
        }
        #endregion

    }

    internal class GetStream : IDisposable
    {
        private GCHandle gch;
        private Stream ms = new MemoryStream();
        public string url { get; set; }
        public WaveStream blockAlignedStream
        {
            get { return Wave(); }
            set { }
        }

        public WaveStream Wave()
        {
            #region Download
            new Thread(delegate(object o)
            {
                var response = HttpWebRequest.Create(url).GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[65536]; // 64KB chunks
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var pos = ms.Position;
                        ms.Position = ms.Length;
                        ms.Write(buffer, 0, read);
                        ms.Position = pos;
                    }
                }
            }).Start();

            // Pre-buffering some data to allow NAudio to start playing
            while (ms.Length < 65536 * 10)
            {
                Thread.Sleep(1000);
                //playbackState = StreamingPlaybackState.Buffering;
            }
            if (ms.Length > 65536 * 10)
            {
                //playbackState = StreamingPlaybackState.Playing;
            }
            #endregion

            ms.Position = 0;
            return blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms)));

        }

        private void Close()
        {
            if (this.gch.IsAllocated)
            {
                this.gch.Free();
            }
        }

        public void Dispose()
        {
            this.Close();
            GC.SuppressFinalize(this);
        }
    }

    internal class TokenId
    {
        public static string Token()
        {
            #region Dir
            // Проверка файла на существование.
            if (!Directory.Exists(Constants.dir))
            {
                Directory.CreateDirectory(Constants.dir);
            }
            if (!File.Exists(Constants.path_data))
            {
                using (FileStream stream = File.Create(Constants.path_data)) { }
            }
            #endregion

            // Чтение параметров.
            using (StreamReader sr = new StreamReader(Constants.path_data, Encoding.UTF8))
            {
                string crypted_id = sr.ReadLine();

                if (crypted_id != null)
                {
                    Crypto cr = new Crypto();
                    return cr.Decrypt(sr.ReadLine(), "ididitjustforlulz");
                }
                else
                {
                    return null;
                }
            }
        }

        public static string Id()
        {
            #region Dir
            // Проверка файла на существование.
            if (!Directory.Exists(Constants.dir))
            {
                Directory.CreateDirectory(Constants.dir);
            }
            if (!File.Exists(Constants.path_data))
            {
                using (FileStream stream = File.Create(Constants.path_data)) { }
            }
            #endregion

            // Чтение параметров.
            using (StreamReader sr = new StreamReader(Constants.path_data, Encoding.UTF8))
            {
                string crypted_id = sr.ReadLine();

                if (crypted_id != null)
                {
                    Crypto cr = new Crypto();
                    return cr.Decrypt(crypted_id, "ididitjustforlulz");
                }
                else
                {
                    return null;
                }
            }
        }

    }

}
