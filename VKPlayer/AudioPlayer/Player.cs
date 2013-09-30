using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.Wave;
using PluginVK;

namespace PlayerVK
{
    //To do:
    //Save mp3 to disk while playing from url.
    //Check if mp3 is saved and play local.
    //Play next mp3 after previous
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
        public static int Time;

        private static bool arrayExists
        {
            get
            {
                if (_array != null) return true;
                else return false;
            }
        }
        private static int numb;
        private static string[] _array;
        private static string[] array
        {
            get
            {
                if (arrayExists) return _array;
                else
                {
                    au.token = Token;
                    au.id = Id;
                    _array = au.AudioList();
                    return _array;
                }
            }
        }
        private static string url
        {
            get
            {
                return array[numb].Split('#')[4];
            }
        }

        #region GetString()
        public static string Artist
        {
            get
            {
                if (arrayExists) return array[numb].Split('#')[1];
                else return null; 
            }
        }
        public static string Title
        {
            get
            {
                if (arrayExists) return array[numb].Split('#')[2];
                else return null;

            }
        }
        public static int Duration
        {
            get
            {
                if (arrayExists) return Convert.ToInt32(array[numb].Split('#')[3]);
                else return 0;
            }
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
                play();
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
        private static void play()
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

}
