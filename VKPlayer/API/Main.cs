using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PluginVK;
using PlayerVK;

namespace Rainmeter
{
    internal class Measure
    {

        enum AudioPlayer
        {
            Player,
            Artist,
            Title,
            Duration
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
                case "artist":
                    Type = AudioPlayer.Artist;
                    break;
                case "title":
                    Type = AudioPlayer.Title;
                    break;
                case "duration":
                    Type = AudioPlayer.Duration;
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
                    if (Player.time == 0)
                    {
                        Player.time = Player.Duration;
                        return (double)Player.time;
                    }
                    else
                    {
                        if (Player.time > 1)
                        {
                            if (Player.option != Player.Playing.Pause)
                            {
                                Player.time--;
                            }
                            else 
                            { 
                                return (double)Player.time;
                            }
                        }
                        return (double)Player.time;
                    }
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
                    if (Player.Artist == null)
                    {
                        return "Not Authorized";
                    }
                    else
                    {
                        return Player.Artist;
                    }

                case AudioPlayer.Title:
                    if (Player.Title == null)
                    {
                        return "Click Play";
                    }
                    else
                    {
                        return Player.Title;
                    }
            }
            return null;
        }

        internal void ExecuteBang(string Command)
        {
            Verification.Audio(Command);
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
