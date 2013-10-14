using System;
using System.Collections.Generic;
using Rainmeter.AudioPlayer;

namespace Rainmeter.API
{
    internal class Measure
    {
        private AudioPlayer _type;

        internal void Reload(API rm, ref double maxValue)
        {
            string type = rm.ReadString("PlayerType", "");
            switch (type.ToLowerInvariant())
            {
                case "status":
                    _type = AudioPlayer.Status;
                    break;
                case "state":
                    _type = AudioPlayer.State;
                    break;
                case "artist":
                    _type = AudioPlayer.Artist;
                    break;
                case "title":
                    _type = AudioPlayer.Title;
                    break;
                case "duration":
                    _type = AudioPlayer.Duration;
                    break;
                case "position":
                    _type = AudioPlayer.Position;
                    break;
                case "repeat":
                    _type = AudioPlayer.Repeat;
                    break;
                case "shuffle":
                    _type = AudioPlayer.Shuffle;
                    break;
                case "volume":
                    _type = AudioPlayer.Volume;
                    break;
                case "progress":
                    _type = AudioPlayer.Progress;
                    break;
                default:
                    API.Log(API.LogType.Error, "VKPlugin.dll PlayerType=" + type + " not valid");
                    break;
            }
        }

        internal void Initialize()
        {
        }

        internal double Update()
        {
            switch (_type)
            {
                case AudioPlayer.Duration:
                    return Player.Duration;

                case AudioPlayer.Position:
                    if (Player.Played) Player.NextCheck(); //find better methode.
                    return Math.Round(Player.Position);

                case AudioPlayer.State:
                    return Player.State;

                case AudioPlayer.Repeat:
                    if (Player.Repeat) return 0.0;
                    return 1.0;

                case AudioPlayer.Shuffle:
                    if (Player.Shuffle) return 0.0;
                    return 1.0;

                case AudioPlayer.Progress:
                    return Player.Progress;
            }
            return 0.0;
        }

        internal string GetString()
        {
            switch (_type)
            {
                case AudioPlayer.Status:
                    return "VKPlayer by Aragas (Aragasas)";

                case AudioPlayer.Artist:
                    if (Player.Artist == null) return "Not Authorized";
                    return Player.Artist;

                case AudioPlayer.Title:
                    if (Player.Title == null) return "Click Play";
                    return Player.Title;
            }
            return null;
        }

        internal void ExecuteBang(string сommand)
        {
            Verification.StartExecute(сommand);
        }

        private enum AudioPlayer
        {
            Status,
            Artist,
            Title,
            Duration,
            Position,
            State,
            Repeat,
            Shuffle,
            Volume,
            Progress
        }
    }

    #region Plugin

    public static class Plugin
    {
        internal static Dictionary<uint, Measure> Measures = new Dictionary<uint, Measure>();

        [DllExport]
        public static unsafe void Initialize(void** data, void* rm)
        {
            var id = (uint) *data;
            Measures.Add(id, new Measure());
        }

        [DllExport]
        public static unsafe void Finalize(void* data)
        {
            var id = (uint) data;
            Measures.Remove(id);
        }

        [DllExport]
        public static unsafe void Reload(void* data, void* rm, double* maxValue)
        {
            var id = (uint) data;
            Measures[id].Reload(new API((IntPtr) rm), ref *maxValue);
        }

        [DllExport]
        public static unsafe double Update(void* data)
        {
            var id = (uint) data;
            return Measures[id].Update();
        }

        [DllExport]
        public static unsafe char* GetString(void* data)
        {
            var id = (uint) data;
            fixed (char* s = Measures[id].GetString()) return s;
        }

        [DllExport]
        public static unsafe void ExecuteBang(void* data, char* args)
        {
            var id = (uint) data;
            Measures[id].ExecuteBang(new string(args));
        }
    }

    #endregion
}