﻿using System;

namespace NAudio.Wave
{
    /// <summary>
    ///     Utility class to intercept audio from an IWaveProvider and
    ///     save it to disk
    /// </summary>
    public class WaveRecorder : IWaveProvider, IDisposable
    {
        private readonly IWaveProvider source;
        private WaveFileWriter writer;

        /// <summary>
        ///     Constructs a new WaveRecorder
        /// </summary>
        /// <param name="destination">The location to write the WAV file to</param>
        /// <param name="source">The Source Wave Provider</param>
        public WaveRecorder(IWaveProvider source, string destination)
        {
            this.source = source;
            writer = new WaveFileWriter(destination, source.WaveFormat);
        }

        /// <summary>
        ///     Closes the WAV file
        /// </summary>
        public void Dispose()
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }

        /// <summary>
        ///     Read simply returns what the source returns, but writes to disk along the way
        /// </summary>
        public int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = source.Read(buffer, offset, count);
            writer.Write(buffer, offset, bytesRead);
            return bytesRead;
        }

        /// <summary>
        ///     The WaveFormat
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }
    }
}