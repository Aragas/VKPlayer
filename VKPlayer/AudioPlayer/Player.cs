using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.Wave;
using Rainmeter.Methods;

namespace Rainmeter.AudioPlayer
{
    // To do:
    // Save mp3 to disk while playing from url.
    // Check if mp3 is saved and play local.
    // Play next mp3 after previous (find better method).

    /// <summary>
    /// AudioPlayer
    /// </summary>
    public static class Player
    {
        internal enum Playing
        {
            Init,
            Buffering,
            Ready
        }

        internal static Playing Option = Playing.Init;
        internal static WaveChannel32 AudioStream;
        private static GetStream _gStream = new GetStream();
        private static WaveOut _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
        private static readonly Audio Au = new Audio();

        /// <summary>
        /// Audiofile was played.
        /// </summary>
        public static bool Played
        {
            get
            {
                if (Option != Playing.Ready) return false;
                return AudioStream != null && (Duration < AudioStream.CurrentTime.TotalSeconds);
            }
        }

        #region Internal

        private static string _token;
        private static string _id;
        private static string[] _array;
        private static int _numb;

        private static bool ArrayExists
        {
            get
            {
                return _array != null;
            }
        }

        private static string[] Array
        {
            get
            {
                if (ArrayExists) return _array;
                Au.Token = _token;
                Au.Id = _id;
                _array = Au.AudioList();
                return _array;
            }
        }

        private static string Url
        {
            get
            {
                return Array[_numb].Split('#')[4];
            }
        }

        #endregion

        #region Variables

        public static bool Repeat;
        public static bool Shuffle;

        public static string Artist
        {
            get
            {
                if (ArrayExists) return Array[_numb].Split('#')[1];
                return null;
            }
        }

        public static string Title
        {
            get
            {
                if (ArrayExists) return Array[_numb].Split('#')[2];
                return null;
            }
        }

        public static string NextArtist
        {
            get
            {
                if (!ArrayExists) return null;
                if (_numb < Array.Length) return Array[_numb + 1].Split('#')[1];
                return null;
            }
        }

        public static string NextTitle
        {
            get
            {
                if (!ArrayExists) return null;
                if (_numb < Array.Length) return Array[_numb + 1].Split('#')[2];
                return null;
            }
        }

        public static double Duration
        {
            get
            {
                if (ArrayExists) return Convert.ToInt32(Array[_numb].Split('#')[3]);
                return 0.0;
            }
        }

        public static double State
        {
            get
            {
                if (_waveOut.PlaybackState == PlaybackState.Playing) return 1.0;
                if (_waveOut.PlaybackState == PlaybackState.Paused) return 2.0;
                return 0.0;
            }
        }

        public static double Position
        {
            get
            {
                if (_waveOut.PlaybackState == PlaybackState.Stopped) return 0.0;
                if (!Played)return AudioStream.CurrentTime.TotalSeconds;
                return 0.0;
            }
        }

        public static double Progress
        {
            get
            {
                if (Option == Playing.Ready)
                {
                    return Position/Duration;
                }
                return 0.0;
            }
        }

        #endregion

        #region Execute

        /// <summary>
        /// Execute your command.
        /// </summary>
        /// <param name="command">Your command.</param>
        /// <param name="token">Your token.</param>
        /// <param name="id">Your id.</param>
        public static void Execute(string command, string token, string id)
        {
            _token = token;
            _id = id;

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
            else return;
        }

        /// <summary>
        /// Check if audiofile has ended. If true, starts next file. Better put in a loop.
        /// </summary>
        public static void PlayNext()
        {
            if (Repeat)
            {
                Stop();
                Play();
            }
            else if (Shuffle)
            {
                if (_waveOut.PlaybackState == PlaybackState.Stopped) return;
                if (_numb > Array.Length) return;

                var random = new Random();
                _numb = random.Next(0, Array.Length);

                Stop();
                _waveOut.Dispose();
                _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
                _gStream.Dispose();
                _gStream = new GetStream();
                Play();
            }
            else Next();
        }

        private static void PlayPause()
        {
            switch (Option)
            {
                case Playing.Ready:

                    switch (_waveOut.PlaybackState)
                    {
                        case PlaybackState.Playing:
                            Pause();
                            break;
                        case PlaybackState.Paused:
                            _play();
                            break;
                        case PlaybackState.Stopped:
                            Play();
                            break;
                    }
                    break;

                case Playing.Init:
                {
                    #region Thread

                    ThreadStart playStart = delegate
                    {
                        try
                        {
                            Play();
                        }
                        catch{}
                    };
                    new Thread(playStart).Start();

                    #endregion
                }
                    break;
            }
        }

        private static void Stop()
        {
            _waveOut.Stop();
        }

        private static void Next()
        {
            if (_waveOut.PlaybackState == PlaybackState.Stopped) return;
            if (_numb >= Array.Length) return;
            _numb += 1;

            Stop();
            _waveOut.Dispose();
            _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _gStream.Dispose();
            _gStream = null;
            _gStream = new GetStream();
            Play();
        }

        private static void Previous()
        {
            if (_waveOut.PlaybackState == PlaybackState.Stopped) return;
            if (_numb <= 0) return;
            _numb -= 1;

            Stop();
            _waveOut.Dispose();
            _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _gStream.Dispose();
            _gStream = null;
            _gStream = new GetStream();
            Play();
        }

        private static void SetVolume(string value)
        {
#if DEBUG
            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                value = value.Substring(1);
                AudioStream.Volume += Convert.ToSingle(Convert.ToInt32(value)/100);
            }
            else
            {
                AudioStream.Volume = Convert.ToSingle(Convert.ToInt32(value)/100);
            }
#else
            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                try
                {
                    value = value.Substring(1);
                    AudioStream.Volume += Convert.ToSingle(Convert.ToInt32(value)/100);
                }
                catch{}
            }
            else
            {
                try
                {
                    AudioStream.Volume = Convert.ToSingle(Convert.ToInt32(value)/100);
                }
                catch{}
            }
#endif
        }

        private static void SetRepeat(string value)
        {
            switch (value)
            {
                case "1":
                    Repeat = true;
                    break;

                case "0":
                    Repeat = false;
                    break;

                case "-1":
                    if (Repeat) Repeat = false;
                    else
                    {
                        Repeat = true;
                        Shuffle = false;
                    }
                    break;
            }
        }

        private static void SetShuffle(string value)
        {
            switch (value)
            {
                case "1":
                    Shuffle = true;
                    break;

                case "0":
                    Shuffle = false;
                    break;

                case "-1":
                    if (Shuffle) Shuffle = false;
                    else
                    {
                        Shuffle = true;
                        Repeat = false;
                    }
                    break;
            }
        }

        private static void SetPosition(string value)
        {
#if DEBUG
            if (Option != Playing.Ready) return;
            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                bool plus = (value.Contains("+"));
                value = value.Substring(1);
                double seconds = Convert.ToDouble(value)/100.0*Duration;

                if (plus) AudioStream.CurrentTime = AudioStream.CurrentTime.Add(TimeSpan.FromSeconds(seconds));
                else AudioStream.CurrentTime = AudioStream.CurrentTime.Subtract(TimeSpan.FromSeconds(seconds));
            }
            else
            {
                double seconds = Convert.ToDouble(value)/100.0*Duration;
                AudioStream.CurrentTime = TimeSpan.FromSeconds(seconds);
            }
#else
            if (Option != Playing.Ready) return;
            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                try
                {
                    bool plus = (value.Contains("+"));
                    value = value.Substring(1);
                    double seconds = Convert.ToDouble(value)/100.0*Duration;

                    if (plus) AudioStream.CurrentTime = AudioStream.CurrentTime.Add(TimeSpan.FromSeconds(seconds));
                    else AudioStream.CurrentTime = AudioStream.CurrentTime.Subtract(TimeSpan.FromSeconds(seconds));
                }
                catch{}
            }
            else
            {
                try
                {
                    double seconds = Convert.ToDouble(value)/100.0*Duration;
                    AudioStream.CurrentTime = TimeSpan.FromSeconds(seconds);
                }
                catch{}
            }
#endif
        }

        private static void Play()
        {
            _gStream.Url = Url;

            _waveOut.Init(_gStream.Wave());
            AudioStream.Volume = 0.7F;
            _waveOut.Play();
        }

        private static void _play()
        {
            _waveOut.Play();
        }

        private static void Pause()
        {
            _waveOut.Pause();
        }

        #endregion
    }

    internal class GetStream : IDisposable
    {
        private Stream _ms = new MemoryStream();
        private Mp3FileReader _reader;
        private WaveChannel32 _channel;

        public string Url { private get; set; }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }
            if (_ms != null)
            {
                _ms.Dispose();
                _ms = null;
            }

            GC.SuppressFinalize(this);
        }

        public WaveStream Wave()
        {
            #region Download

                var response = WebRequest.Create(Url).GetResponse();
                using (var stream = response.GetResponseStream())
                {
                    var buffer = new byte[65536]; // 64KB chunks
                    int read;
                    while (stream != null && (read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var pos = _ms.Position;
                        _ms.Position = _ms.Length;
                        _ms.Write(buffer, 0, read);
                        _ms.Position = pos;
                    }
                }

            // Pre-buffering some data to allow NAudio to start playing
            while (_ms.Length < 65536 * 10)
            {
                if (Player.Option != Player.Playing.Buffering) Player.Option = Player.Playing.Buffering;

                Thread.Sleep(500);
            }
            if (_ms.Length > 65536 * 10) Player.Option = Player.Playing.Ready;

            #endregion

            _ms.Position = 0;
            _reader = new Mp3FileReader(_ms);
            _channel = new WaveChannel32(_reader);
            Player.AudioStream = _channel;
            return Player.AudioStream;
        }
    }
}