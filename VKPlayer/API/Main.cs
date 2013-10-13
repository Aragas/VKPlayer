using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VKPlayer;

namespace Rainmeter
{
    internal class Measure
    {

        enum AudioPlayer
        {
            Player,
            Artist,
            Title,
            Duration,
            Time,
            State,
            Repeat,
            Shuffle
        }
        AudioPlayer Type;

        internal Measure()
        {
        }

        internal void Reload(Rainmeter.API rm, ref double maxValue)
        {
            string type = rm.ReadString("Type", "");
            switch (type.ToLowerInvariant())
            {
                case "player":
                    Type = AudioPlayer.Player;
                    break;
                case "state":
                    Type = AudioPlayer.State;
                    break;
                case "artist":
                    Type = AudioPlayer.Artist;
                    break;
                case "title":
                    Type = AudioPlayer.Title;
                    break;
                case "duration":
                    Type = AudioPlayer.Duration;
                    break;
                case "time":
                    Type = AudioPlayer.Time;
                    break;
                case "repeat":
                    Type = AudioPlayer.Repeat;
                    break;
                case "shuffle":
                    Type = AudioPlayer.Shuffle;
                    break;
                default:
                    API.Log(API.LogType.Error, "VKPlugin.dll Type=" + type + " not valid");
                    break;
            }

        }

        internal void Initialize()
        {
        }

        internal double Update()
        {
            switch (Type)
            {
                case AudioPlayer.Duration:
                    return Player.Duration;

                case AudioPlayer.Time:
                    if (Player.Played) Player.NextCheck(); //find better methode.
                    return Math.Round(Player.Time);

                case AudioPlayer.State:
                    return (double)Player.State;

                case AudioPlayer.Repeat:
                    if (Player.Repeat) return 255.0;
                    else return 128.0;

                case AudioPlayer.Shuffle:
                    if (Player.Shuffle) return 255.0;
                    else return 128.0;
            }
            return 0.0;
        }

        internal string GetString()
        {
            switch (Type)
            {
                case AudioPlayer.Player:
                    return "VKPlayer by Aragas (Aragasas)";

                case AudioPlayer.Artist:
                    if (Player.Artist == null) return "Not Authorized";
                    else return Player.Artist;

                case AudioPlayer.Title:
                    if (Player.Title == null) return "Click Play";
                    else return Player.Title;

            }
            return null;
        }

        internal void ExecuteBang(string Command)
        {
            Verification.StartExecute(Command);
            return;
        }

    }

    #region Plugin

    public static class Plugin
    {
        internal static Dictionary<uint, Measure> Measures = new Dictionary<uint, Measure>();

        [DllExport]
        public unsafe static void Initialize(void** data, void* rm)
        {
            uint id = (uint)((void*)*data);
            Measures.Add(id, new Measure());
        }

        [DllExport]
        public unsafe static void Finalize(void* data)
        {
            uint id = (uint)data;
            Measures.Remove(id);
        }

        [DllExport]
        public unsafe static void Reload(void* data, void* rm, double* maxValue)
        {
            uint id = (uint)data;
            Measures[id].Reload(new Rainmeter.API((IntPtr)rm), ref *maxValue);
        }

        [DllExport]
        public unsafe static double Update(void* data)
        {
            uint id = (uint)data;
            return Measures[id].Update();
        }

        [DllExport]
        public unsafe static char* GetString(void* data)
        {
            uint id = (uint)data;
            fixed (char* s = Measures[id].GetString()) return s;
        }

        [DllExport]
        public unsafe static void ExecuteBang(void* data, char* args)
        {
            uint id = (uint)data;
            Measures[id].ExecuteBang(new string(args));
        }

    }
    #endregion
}
