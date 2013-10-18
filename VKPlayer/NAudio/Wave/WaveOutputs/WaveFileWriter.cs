using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using NAudio.Wave.SampleProviders;

namespace NAudio.Wave
{
    /// <summary>
    ///     This class writes WAV data to a .wav file on disk
    /// </summary>
    public class WaveFileWriter : Stream
    {
        private readonly string filename;
        private readonly WaveFormat format;
        private readonly byte[] value24 = new byte[3]; // keep this around to save us creating it every time
        private readonly BinaryWriter writer;
        private int dataChunkSize;
        private long dataSizePos;
        private long factSampleCountPos;
        private Stream outStream;

        /// <summary>
        ///     WaveFileWriter that actually writes to a stream
        /// </summary>
        /// <param name="outStream">Stream to be written to</param>
        /// <param name="format">Wave format to use</param>
        public WaveFileWriter(Stream outStream, WaveFormat format)
        {
            this.outStream = outStream;
            this.format = format;
            writer = new BinaryWriter(outStream, Encoding.ASCII);
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(0); // placeholder
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            format.Serialize(writer);

            CreateFactChunk();
            WriteDataChunkHeader();
        }

        /// <summary>
        ///     Creates a new WaveFileWriter
        /// </summary>
        /// <param name="filename">The filename to write to</param>
        /// <param name="format">The Wave Format of the output data</param>
        public WaveFileWriter(string filename, WaveFormat format)
            : this(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read), format)
        {
            this.filename = filename;
        }

        /// <summary>
        ///     The wave file name or null if not applicable
        /// </summary>
        public string Filename
        {
            get { return filename; }
        }

        /// <summary>
        ///     Number of bytes of audio in the data chunk
        /// </summary>
        public override long Length
        {
            get { return dataChunkSize; }
        }

        /// <summary>
        ///     WaveFormat of this wave file
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return format; }
        }

        /// <summary>
        ///     Returns false: Cannot read from a WaveFileWriter
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        ///     Returns true: Can write to a WaveFileWriter
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        ///     Returns false: Cannot seek within a WaveFileWriter
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets the Position in the WaveFile (i.e. number of bytes written so far)
        /// </summary>
        public override long Position
        {
            get { return dataChunkSize; }
            set { throw new InvalidOperationException("Repositioning a WaveFileWriter is not supported"); }
        }

        /// <summary>
        ///     Creates a 16 bit Wave File from an ISampleProvider
        ///     BEWARE: the source provider must not return data indefinitely
        /// </summary>
        /// <param name="filename">The filename to write to</param>
        /// <param name="sourceProvider">The source sample provider</param>
        public static void CreateWaveFile16(string filename, ISampleProvider sourceProvider)
        {
            CreateWaveFile(filename, new SampleToWaveProvider16(sourceProvider));
        }

        /// <summary>
        ///     Creates a Wave file by reading all the data from a WaveProvider
        ///     BEWARE: the WaveProvider MUST return 0 from its Read method when it is finished,
        ///     or the Wave File will grow indefinitely.
        /// </summary>
        /// <param name="filename">The filename to use</param>
        /// <param name="sourceProvider">The source WaveProvider</param>
        public static void CreateWaveFile(string filename, IWaveProvider sourceProvider)
        {
            using (var writer = new WaveFileWriter(filename, sourceProvider.WaveFormat))
            {
                long outputLength = 0;
                var buffer = new byte[sourceProvider.WaveFormat.AverageBytesPerSecond*4];
                while (true)
                {
                    int bytesRead = sourceProvider.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // end of source provider
                        break;
                    }
                    outputLength += bytesRead;
                    if (outputLength > Int32.MaxValue)
                    {
                        throw new InvalidOperationException(
                            "WAV File cannot be greater than 2GB. Check that sourceProvider is not an endless stream.");
                    }
                    writer.Write(buffer, 0, bytesRead);
                }
            }
        }

        private void WriteDataChunkHeader()
        {
            writer.Write(Encoding.ASCII.GetBytes("data"));
            dataSizePos = outStream.Position;
            writer.Write(0); // placeholder
        }

        private void CreateFactChunk()
        {
            if (HasFactChunk())
            {
                writer.Write(Encoding.ASCII.GetBytes("fact"));
                writer.Write(4);
                factSampleCountPos = outStream.Position;
                writer.Write(0); // number of samples
            }
        }

        private bool HasFactChunk()
        {
            return format.Encoding != WaveFormatEncoding.Pcm && format.BitsPerSample != 0;
        }

        /// <summary>
        ///     Read is not supported for a WaveFileWriter
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Cannot read from a WaveFileWriter");
        }

        /// <summary>
        ///     Seek is not supported for a WaveFileWriter
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException("Cannot seek within a WaveFileWriter");
        }

        /// <summary>
        ///     SetLength is not supported for WaveFileWriter
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Cannot set length of a WaveFileWriter");
        }

        /// <summary>
        ///     Appends bytes to the WaveFile (assumes they are already in the correct format)
        /// </summary>
        /// <param name="data">the buffer containing the wave data</param>
        /// <param name="offset">the offset from which to start writing</param>
        /// <param name="count">the number of bytes to write</param>
        [Obsolete("Use Write instead")]
        public void WriteData(byte[] data, int offset, int count)
        {
            Write(data, offset, count);
        }

        /// <summary>
        ///     Appends bytes to the WaveFile (assumes they are already in the correct format)
        /// </summary>
        /// <param name="data">the buffer containing the wave data</param>
        /// <param name="offset">the offset from which to start writing</param>
        /// <param name="count">the number of bytes to write</param>
        public override void Write(byte[] data, int offset, int count)
        {
            outStream.Write(data, offset, count);
            dataChunkSize += count;
        }

        /// <summary>
        ///     Writes a single sample to the Wave file
        /// </summary>
        /// <param name="sample">the sample to write (assumed floating point with 1.0f as max value)</param>
        public void WriteSample(float sample)
        {
            if (WaveFormat.BitsPerSample == 16)
            {
                writer.Write((Int16) (Int16.MaxValue*sample));
                dataChunkSize += 2;
            }
            else if (WaveFormat.BitsPerSample == 24)
            {
                byte[] value = BitConverter.GetBytes((Int32) (Int32.MaxValue*sample));
                value24[0] = value[1];
                value24[1] = value[2];
                value24[2] = value[3];
                writer.Write(value24);
                dataChunkSize += 3;
            }
            else if (WaveFormat.BitsPerSample == 32 && WaveFormat.Encoding == WaveFormatEncoding.Extensible)
            {
                writer.Write(UInt16.MaxValue*(Int32) sample);
                dataChunkSize += 4;
            }
            else if (WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                writer.Write(sample);
                dataChunkSize += 4;
            }
            else
            {
                throw new ApplicationException("Only 16, 24 or 32 bit PCM or IEEE float audio data supported");
            }
        }

        /// <summary>
        ///     Writes 32 bit floating point samples to the Wave file
        ///     They will be converted to the appropriate bit depth depending on the WaveFormat of the WAV file
        /// </summary>
        /// <param name="samples">The buffer containing the floating point samples</param>
        /// <param name="offset">The offset from which to start writing</param>
        /// <param name="count">The number of floating point samples to write</param>
        public void WriteSamples(float[] samples, int offset, int count)
        {
            for (int n = 0; n < count; n++)
            {
                WriteSample(samples[offset + n]);
            }
        }

        /// <summary>
        ///     Writes 16 bit samples to the Wave file
        /// </summary>
        /// <param name="samples">The buffer containing the 16 bit samples</param>
        /// <param name="offset">The offset from which to start writing</param>
        /// <param name="count">The number of 16 bit samples to write</param>
        [Obsolete("Use WriteSamples instead")]
        public void WriteData(short[] samples, int offset, int count)
        {
            WriteSamples(samples, offset, count);
        }


        /// <summary>
        ///     Writes 16 bit samples to the Wave file
        /// </summary>
        /// <param name="samples">The buffer containing the 16 bit samples</param>
        /// <param name="offset">The offset from which to start writing</param>
        /// <param name="count">The number of 16 bit samples to write</param>
        public void WriteSamples(short[] samples, int offset, int count)
        {
            // 16 bit PCM data
            if (WaveFormat.BitsPerSample == 16)
            {
                for (int sample = 0; sample < count; sample++)
                {
                    writer.Write(samples[sample + offset]);
                }
                dataChunkSize += (count*2);
            }
                // 24 bit PCM data
            else if (WaveFormat.BitsPerSample == 24)
            {
                byte[] value;
                for (int sample = 0; sample < count; sample++)
                {
                    value = BitConverter.GetBytes(UInt16.MaxValue*samples[sample + offset]);
                    value24[0] = value[1];
                    value24[1] = value[2];
                    value24[2] = value[3];
                    writer.Write(value24);
                }
                dataChunkSize += (count*3);
            }
                // 32 bit PCM data
            else if (WaveFormat.BitsPerSample == 32 && WaveFormat.Encoding == WaveFormatEncoding.Extensible)
            {
                for (int sample = 0; sample < count; sample++)
                {
                    writer.Write(UInt16.MaxValue*samples[sample + offset]);
                }
                dataChunkSize += (count*4);
            }
                // IEEE float data
            else if (WaveFormat.BitsPerSample == 32 && WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                for (int sample = 0; sample < count; sample++)
                {
                    writer.Write(samples[sample + offset]/(float) (Int16.MaxValue + 1));
                }
                dataChunkSize += (count*4);
            }
            else
            {
                throw new ApplicationException("Only 16, 24 or 32 bit PCM or IEEE float audio data supported");
            }
        }

        /// <summary>
        ///     Ensures data is written to disk
        /// </summary>
        public override void Flush()
        {
            writer.Flush();
        }

        #region IDisposable Members

        /// <summary>
        ///     Actually performs the close,making sure the header contains the correct data
        /// </summary>
        /// <param name="disposing">True if called from <see>Dispose</see></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (outStream != null)
                {
                    try
                    {
                        UpdateHeader(writer);
                    }
                    finally
                    {
                        // in a finally block as we don't want the FileStream to run its disposer in
                        // the GC thread if the code above caused an IOException (e.g. due to disk full)
                        outStream.Close(); // will close the underlying base stream
                        outStream = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Updates the header with file size information
        /// </summary>
        protected virtual void UpdateHeader(BinaryWriter writer)
        {
            Flush();
            UpdateRiffChunk(writer);
            UpdateFactChunk(writer);
            UpdateDataChunk(writer);
        }

        private void UpdateDataChunk(BinaryWriter writer)
        {
            writer.Seek((int) dataSizePos, SeekOrigin.Begin);
            writer.Write(dataChunkSize);
        }

        private void UpdateRiffChunk(BinaryWriter writer)
        {
            writer.Seek(4, SeekOrigin.Begin);
            writer.Write((int) (outStream.Length - 8));
        }

        private void UpdateFactChunk(BinaryWriter writer)
        {
            if (HasFactChunk())
            {
                int bitsPerSample = (format.BitsPerSample*format.Channels);
                if (bitsPerSample != 0)
                {
                    writer.Seek((int) factSampleCountPos, SeekOrigin.Begin);
                    writer.Write((dataChunkSize*8)/bitsPerSample);
                }
            }
        }

        /// <summary>
        ///     Finaliser - should only be called if the user forgot to close this WaveFileWriter
        /// </summary>
        ~WaveFileWriter()
        {
            Debug.Assert(false, "WaveFileWriter was not disposed");
            Dispose(false);
        }

        #endregion
    }
}