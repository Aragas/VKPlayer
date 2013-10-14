﻿using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.Wave;
using System.ComponentModel;

namespace VKPlayer
{
    //To do:
    //Save mp3 to disk while playing from url.
    //Check if mp3 is saved and play local.
    //Play next mp3 after previous (find better methode)

    public class Player
    {
        public enum Playing
        {
            Init,
            Buffering,
            Ready
        }
        public static Playing option = Playing.Init;

        internal static WaveChannel32 volumeStream;
        static GetStream gStream = new GetStream();
        static WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
        static Audio au = new Audio();

        public static string Token;
        public static string Id;
        public static bool Played
        {
            get
            {
                if (option == Playing.Ready)
                {
                    if (Duration > volumeStream.CurrentTime.TotalSeconds) return false;
                    else return true;
                }
                else return false;
            }
        }

        #region Internal
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
        #endregion

        #region Variables
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
        public static double Duration
        {
            get
            {
                if (arrayExists) return Convert.ToInt32(array[numb].Split('#')[3]);
                else return 0.0;
            }
        }
        public static double State
        {
            get
            {
                if (waveOut.PlaybackState == PlaybackState.Playing) return 1.0;
                else if (waveOut.PlaybackState == PlaybackState.Paused) return 2.0;
                else return 0.0;
            }
        }
        public static double Position
        {
            get
            {
                if (waveOut.PlaybackState != PlaybackState.Stopped)
                {
                    if (!Played)
                    {
                        return volumeStream.CurrentTime.TotalSeconds;
                    }
                    else return 0.0;
                }
                else return 0.0;
            }
        }
        public static double Progress
        {
            get
            {
                if (option == Playing.Ready)
                {
                    return Position / Duration;
                }
                else return 0.0;
            }
        }
        public static bool Repeat = false;
        public static bool Shuffle = false;
        #endregion

        #region Execute
        public static void Execute(string Command, string token, string id)
        {
            Token = token;
            Id = id;

            if (Command == "PlayPause") PlayPause();
            else if (Command == "Play") PlayPause();
            else if (Command == "Pause") PlayPause();
            else if (Command == "Stop") Stop();
            else if (Command == "Next") Next();
            else if (Command == "Previous") Previous();
            else if (Command.Contains("SetVolume")) SetVolume(Command.Remove(0,10));
            else if (Command.Contains("SetShuffle")) SetShuffle(Command.Remove(0, 11));
            else if (Command.Contains("SetRepeat")) SetRepeat(Command.Remove(0, 10));
            else if (Command.Contains("SetPosition")) SetPosition(Command.Remove(0, 12));
            else if (Command.Contains("SetRating")) SetRating(Command.Remove(0, 10));
            else return;
        }

        public static void PlayPause()
        {
            if (option == Playing.Ready)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing) pause();
                else if (waveOut.PlaybackState == PlaybackState.Paused) play();
                else if (waveOut.PlaybackState == PlaybackState.Stopped) Play();
            }
            else if (option == Playing.Init)
            {
                #region
                ThreadStart work = delegate
                {
                    try
                    {
                        Play();
                    }
                    catch { }
                };
                new Thread(work).Start();
                #endregion
            }
        }
        public static void Stop()
        {
            waveOut.Stop();
        }
        public static void Next()
        {
            if (waveOut.PlaybackState != PlaybackState.Stopped)
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
            if (waveOut.PlaybackState != PlaybackState.Stopped)
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

        public static void SetVolume(string Value)
        {
            if (Value.Contains("+") || Value.Contains("-"))
            {
                Value.Remove(0, 1);
                volumeStream.Volume += Convert.ToSingle(Convert.ToInt32(Value) / 100);
            }
            else
            {
#if DEBUG
                volumeStream.Volume = Convert.ToSingle(Convert.ToInt32(Value) / 100);
#else
                try
                {
                    volumeStream.Volume = Convert.ToSingle(Convert.ToInt32(Value) / 100);
                }
                catch { }
#endif
            }
        }
        public static void SetRepeat(string Value)
        {
            if (Value == "1") Repeat = true;
            else if (Value == "0") Repeat = false;
            else if (Value == "-1")
            {
                if (Repeat)
                {
                    Repeat = false;
                }
                else
                {
                    Repeat = true;
                    Shuffle = false;
                }
            }
            else ;
        }
        public static void SetShuffle(string Value)
        {
            if (Value == "1") Shuffle = true;
            else if (Value == "0") Shuffle = false;
            else if (Value == "-1")
            {
                if (Shuffle)
                {
                    Shuffle = false;
                }
                else
                {
                    Shuffle = true;
                    Repeat = false;
                }
            }
            else ;
        }
        public static void SetPosition(string Value)
        {
            if (Value.Contains("+") || Value.Contains("-"))
            {
                Value.Remove(0, 1);
                //
            }
            else
            {
#if DEBUG
                //
#else
                try
                {
                //
                }
                catch { }
#endif
            }
        }
        public static void SetRating(string Value)
        {
            if (Value == "1" || Value == "2" || Value == "3" ||
                Value == "4" || Value == "5")
            {
            }
        }

        public static void NextCheck()
        {
            if (Repeat)
            {
                Stop();
                Play();
            }
            else if (Shuffle)
            {
                if (waveOut.PlaybackState != PlaybackState.Stopped)
                {
                    if (numb < array.Length)
                    {
                        Random random = new Random();
                        numb = random.Next(0, array.Length);

                        Stop();
                        waveOut.Dispose();
                        waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
                        gStream.Dispose();
                        gStream = new GetStream();
                        Play();
                    }
                }
            }
            else
            {
                Next();
            }
        }

        private static void Play()
        {
            gStream.url = url;

            waveOut.Init(gStream.Wave());
            volumeStream.Volume = 0.7F;
            waveOut.Play();
        }
        private static void play()
        {
            waveOut.Play();
        }
        private static void pause()
        {
            waveOut.Pause();
        }

        #endregion

    }

    internal class GetStream : IDisposable
    {
        private GCHandle gch;
        private Stream ms = new MemoryStream();

        public string url { get; set; }

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
                if (Player.option != Player.Playing.Buffering) Player.option = Player.Playing.Buffering;

                Thread.Sleep(500);
                
            }
            if (ms.Length > 65536 * 10) Player.option = Player.Playing.Ready;
            #endregion

            ms.Position = 0;
            Player.volumeStream = new WaveChannel32(new Mp3FileReader(ms));
            return Player.volumeStream;

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
