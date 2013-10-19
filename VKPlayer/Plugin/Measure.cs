using System;
using Rainmeter.API;
using Rainmeter.AudioPlayer;

namespace Rainmeter.Plugin
{
    internal class Measure
    {
        private AudioPlayer _type;

        internal void Reload(RainmeterAPI rm, ref double maxValue)
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
                    RainmeterAPI.Log(RainmeterAPI.LogType.Error, "VKPlugin.dll PlayerType=" + type + " not valid");
                    break;
            }
        }

        internal double Update()
        {
            switch (_type)
            {
                case AudioPlayer.Duration:
                    return Player.Duration;

                case AudioPlayer.Position:
                    return Math.Round(Player.Position);

                case AudioPlayer.State:
                    if (Player.Played) Player.PlayNext();
                    return Player.State;

                case AudioPlayer.Repeat:
                    return Player.Repeat ? 0.0 : 1.0;

                case AudioPlayer.Shuffle:
                    return Player.Shuffle ? 0.0 : 1.0;

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
                    return Player.Artist ?? "Not Authorized";

                case AudioPlayer.Title:
                    return Player.Title ?? "Click Play";

                case AudioPlayer.NextArtist:
                    return Player.NextArtist ?? "Not Authorized";

                case AudioPlayer.NextTitle:
                    return Player.NextTitle ?? "Click Play";
            }
            return null;
        }

        internal static void ExecuteBang(string command)
        {
            Execute.Start(command);
        }

        private enum AudioPlayer
        {
            Status,
            Artist,
            Title,
            NextArtist,
            NextTitle,
            Duration,
            Position,
            State,
            Repeat,
            Shuffle,
            Volume,
            Progress
        }
    }
}