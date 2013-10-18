﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NAudio.Wave
{
    /// <summary>
    ///     A read-only stream of AIFF data based on an aiff file
    ///     with an associated WaveFormat
    ///     originally contributed to NAudio by Giawa
    /// </summary>
    public class AiffFileReader : WaveStream
    {
        private readonly List<AiffChunk> chunks = new List<AiffChunk>();
        private readonly int dataChunkLength;
        private readonly long dataPosition;
        private readonly bool ownInput;
        private readonly WaveFormat waveFormat;
        private Stream waveStream;

        /// <summary>Supports opening a AIF file</summary>
        /// <remarks>
        ///     The AIF is of similar nastiness to the WAV format.
        ///     This supports basic reading of uncompressed PCM AIF files,
        ///     with 8, 16, 24 and 32 bit PCM data.
        /// </remarks>
        public AiffFileReader(String aiffFile) :
            this(File.OpenRead(aiffFile))
        {
            ownInput = true;
        }

        /// <summary>
        ///     Creates an Aiff File Reader based on an input stream
        /// </summary>
        /// <param name="inputStream">The input stream containing a AIF file including header</param>
        public AiffFileReader(Stream inputStream)
        {
            waveStream = inputStream;
            ReadAiffHeader(waveStream, out waveFormat, out dataPosition, out dataChunkLength, chunks);
            Position = 0;
        }

        /// <summary>
        ///     <see cref="WaveStream.WaveFormat" />
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        /// <summary>
        ///     <see cref="WaveStream.WaveFormat" />
        /// </summary>
        public override long Length
        {
            get { return dataChunkLength; }
        }

        /// <summary>
        ///     Number of Samples (if possible to calculate)
        /// </summary>
        public long SampleCount
        {
            get
            {
                if (waveFormat.Encoding == WaveFormatEncoding.Pcm ||
                    waveFormat.Encoding == WaveFormatEncoding.Extensible ||
                    waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                {
                    return dataChunkLength/BlockAlign;
                }
                throw new FormatException("Sample count is calculated only for the standard encodings");
            }
        }

        /// <summary>
        ///     Position in the AIFF file
        ///     <see cref="Stream.Position" />
        /// </summary>
        public override long Position
        {
            get { return waveStream.Position - dataPosition; }
            set
            {
                lock (this)
                {
                    value = Math.Min(value, Length);
                    // make sure we don't get out of sync
                    value -= (value%waveFormat.BlockAlign);
                    waveStream.Position = value + dataPosition;
                }
            }
        }

        /// <summary>
        ///     Ensures valid AIFF header and then finds data offset.
        /// </summary>
        /// <param name="stream">The stream, positioned at the start of audio data</param>
        /// <param name="format">The format found</param>
        /// <param name="dataChunkPosition">The position of the data chunk</param>
        /// <param name="dataChunkLength">The length of the data chunk</param>
        /// <param name="chunks">Additional chunks found</param>
        public static void ReadAiffHeader(Stream stream, out WaveFormat format, out long dataChunkPosition,
            out int dataChunkLength, List<AiffChunk> chunks)
        {
            dataChunkPosition = -1;
            format = null;
            var br = new BinaryReader(stream);

            if (ReadChunkName(br) != "FORM")
            {
                throw new FormatException("Not an AIFF file - no FORM header.");
            }
            uint fileSize = ConvertInt(br.ReadBytes(4));
            string formType = ReadChunkName(br);
            if (formType != "AIFC" && formType != "AIFF")
            {
                throw new FormatException("Not an AIFF file - no AIFF/AIFC header.");
            }

            dataChunkLength = 0;

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                AiffChunk nextChunk = ReadChunkHeader(br);
                if (nextChunk.ChunkName == "COMM")
                {
                    short numChannels = ConvertShort(br.ReadBytes(2));
                    uint numSampleFrames = ConvertInt(br.ReadBytes(4));
                    short sampleSize = ConvertShort(br.ReadBytes(2));
                    double sampleRate = ConvertExtended(br.ReadBytes(10));

                    format = new WaveFormat((int) sampleRate, sampleSize, numChannels);

                    if (nextChunk.ChunkLength > 18 && formType == "AIFC")
                    {
                        // In an AIFC file, the compression format is tacked on to the COMM chunk
                        string compress = new string(br.ReadChars(4)).ToLower();
                        if (compress != "none") throw new FormatException("Compressed AIFC is not supported.");
                        br.ReadBytes((int) nextChunk.ChunkLength - 22);
                    }
                    else br.ReadBytes((int) nextChunk.ChunkLength - 18);
                }
                else if (nextChunk.ChunkName == "SSND")
                {
                    uint offset = ConvertInt(br.ReadBytes(4));
                    uint blockSize = ConvertInt(br.ReadBytes(4));
                    dataChunkPosition = nextChunk.ChunkStart + 16 + offset;
                    dataChunkLength = (int) nextChunk.ChunkLength - 8;

                    br.ReadBytes((int) nextChunk.ChunkLength - 8);
                }
                else
                {
                    if (chunks != null)
                    {
                        chunks.Add(nextChunk);
                    }
                    br.ReadBytes((int) nextChunk.ChunkLength);
                }

                if (nextChunk.ChunkName == "\0\0\0\0") break;
            }

            if (format == null)
            {
                throw new FormatException("Invalid AIFF file - No COMM chunk found.");
            }
            if (dataChunkPosition == -1)
            {
                throw new FormatException("Invalid AIFF file - No SSND chunk found.");
            }
        }

        /// <summary>
        ///     Cleans up the resources associated with this AiffFileReader
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                if (waveStream != null)
                {
                    // only dispose our source if we created it
                    if (ownInput)
                    {
                        waveStream.Close();
                    }
                    waveStream = null;
                }
            }
            else
            {
                Debug.Assert(false, "AiffFileReader was not disposed");
            }
            // Release unmanaged resources.
            // Set large fields to null.
            // Call Dispose on your base class.
            base.Dispose(disposing);
        }


        /// <summary>
        ///     Reads bytes from the AIFF File
        ///     <see cref="Stream.Read" />
        /// </summary>
        public override int Read(byte[] array, int offset, int count)
        {
            if (count%waveFormat.BlockAlign != 0)
            {
                throw new ApplicationException(
                    String.Format("Must read complete blocks: requested {0}, block align is {1}", count,
                        WaveFormat.BlockAlign));
            }
            // sometimes there is more junk at the end of the file past the data chunk
            if (Position + count > dataChunkLength)
            {
                count = dataChunkLength - (int) Position;
            }

            // Need to fix the endianness since intel expect little endian, and apple is big endian.
            var buffer = new byte[count];
            int length = waveStream.Read(buffer, offset, count);

            int bytesPerSample = WaveFormat.BitsPerSample/8;
            for (int i = 0; i < length; i += bytesPerSample)
            {
                if (WaveFormat.BitsPerSample == 8)
                {
                    array[i] = buffer[i];
                }
                else if (WaveFormat.BitsPerSample == 16)
                {
                    array[i + 0] = buffer[i + 1];
                    array[i + 1] = buffer[i];
                }
                else if (WaveFormat.BitsPerSample == 24)
                {
                    array[i + 0] = buffer[i + 2];
                    array[i + 1] = buffer[i + 1];
                    array[i + 2] = buffer[i + 0];
                }
                else if (WaveFormat.BitsPerSample == 32)
                {
                    array[i + 0] = buffer[i + 3];
                    array[i + 1] = buffer[i + 2];
                    array[i + 2] = buffer[i + 1];
                    array[i + 3] = buffer[i + 0];
                }
                else throw new FormatException("Unsupported PCM format.");
            }

            return length;
        }

        #region Endian Helpers

        private static uint ConvertInt(byte[] buffer)
        {
            if (buffer.Length != 4) throw new Exception("Incorrect length for long.");
            return (uint) ((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3]);
        }

        private static short ConvertShort(byte[] buffer)
        {
            if (buffer.Length != 2) throw new Exception("Incorrect length for int.");
            return (short) ((buffer[0] << 8) | buffer[1]);
        }

        #endregion

        #region IEEE 80-bit Extended

        private static double UnsignedToFloat(ulong u)
        {
            return ((long) (u - 2147483647L - 1) + 2147483648.0);
        }

        private static double ldexp(double x, int exp)
        {
            return x*Math.Pow(2, exp);
        }

        private static double ConvertExtended(byte[] bytes)
        {
            if (bytes.Length != 10) throw new Exception("Incorrect length for IEEE extended.");
            double f;
            int expon;
            uint hiMant, loMant;

            expon = ((bytes[0] & 0x7F) << 8) | bytes[1];
            hiMant = (uint) ((bytes[2] << 24) | (bytes[3] << 16) | (bytes[4] << 8) | bytes[5]);
            loMant = (uint) ((bytes[6] << 24) | (bytes[7] << 16) | (bytes[8] << 8) | bytes[9]);

            if (expon == 0 && hiMant == 0 && loMant == 0)
            {
                f = 0;
            }
            else
            {
                if (expon == 0x7FFF) /* Infinity or NaN */
                {
                    f = double.NaN;
                }
                else
                {
                    expon -= 16383;
                    f = ldexp(UnsignedToFloat(hiMant), expon -= 31);
                    f += ldexp(UnsignedToFloat(loMant), expon -= 32);
                }
            }

            if ((bytes[0] & 0x80) == 0x80) return -f;
            return f;
        }

        #endregion

        #region AiffChunk

        private static AiffChunk ReadChunkHeader(BinaryReader br)
        {
            var chunk = new AiffChunk((uint) br.BaseStream.Position, ReadChunkName(br), ConvertInt(br.ReadBytes(4)));
            return chunk;
        }

        private static string ReadChunkName(BinaryReader br)
        {
            return new string(br.ReadChars(4));
        }

        /// <summary>
        ///     AIFF Chunk
        /// </summary>
        public struct AiffChunk
        {
            /// <summary>
            ///     Chunk Length
            /// </summary>
            public uint ChunkLength;

            /// <summary>
            ///     Chunk Name
            /// </summary>
            public string ChunkName;

            /// <summary>
            ///     Chunk start
            /// </summary>
            public uint ChunkStart;

            /// <summary>
            ///     Creates a new AIFF Chunk
            /// </summary>
            public AiffChunk(uint start, string name, uint length)
            {
                ChunkStart = start;
                ChunkName = name;
                ChunkLength = length + (uint) (length%2 == 1 ? 1 : 0);
            }
        }

        #endregion
    }
}