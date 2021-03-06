﻿using System;
using System.Diagnostics;
using System.Threading;

namespace NAudio.Wave
{
    /// <summary>
    ///     Alternative WaveOut class, making use of the Event callback
    /// </summary>
    public class WaveOutEvent : IWavePlayer
    {
        private readonly AutoResetEvent callbackEvent;
        private readonly SynchronizationContext syncContext;
        private readonly object waveOutLock;
        private WaveOutBuffer[] buffers;
        private IntPtr hWaveOut; // WaveOut handle
        private volatile PlaybackState playbackState;
        private IWaveProvider waveStream;

        /// <summary>
        ///     Opens a WaveOut device
        /// </summary>
        public WaveOutEvent()
        {
            syncContext = SynchronizationContext.Current;
            // set default values up
            DeviceNumber = 0;
            DesiredLatency = 300;
            NumberOfBuffers = 2;

            waveOutLock = new object();
            callbackEvent = new AutoResetEvent(false);
        }

        /// <summary>
        ///     Gets or sets the desired latency in milliseconds
        ///     Should be set before a call to Init
        /// </summary>
        public int DesiredLatency { get; set; }

        /// <summary>
        ///     Gets or sets the number of buffers used
        ///     Should be set before a call to Init
        /// </summary>
        public int NumberOfBuffers { get; set; }

        /// <summary>
        ///     Gets or sets the device number
        ///     Should be set before a call to Init
        ///     This must be between 0 and <see>DeviceCount</see> - 1.
        /// </summary>
        public int DeviceNumber { get; set; }

        /// <summary>
        ///     Indicates playback has stopped automatically
        /// </summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        ///     Initialises the WaveOut device
        /// </summary>
        /// <param name="waveProvider">WaveProvider to play</param>
        public void Init(IWaveProvider waveProvider)
        {
            waveStream = waveProvider;
            int bufferSize =
                waveProvider.WaveFormat.ConvertLatencyToByteSize((DesiredLatency + NumberOfBuffers - 1)/NumberOfBuffers);

            MmResult result;
            lock (waveOutLock)
            {
                result = WaveInterop.waveOutOpenWindow(out hWaveOut, (IntPtr) DeviceNumber, waveStream.WaveFormat,
                    callbackEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero,
                    WaveInterop.WaveInOutOpenFlags.CallbackEvent);
            }
            MmException.Try(result, "waveOutOpen");

            buffers = new WaveOutBuffer[NumberOfBuffers];
            playbackState = PlaybackState.Stopped;
            for (int n = 0; n < NumberOfBuffers; n++)
            {
                buffers[n] = new WaveOutBuffer(hWaveOut, bufferSize, waveStream, waveOutLock);
            }
        }

        /// <summary>
        ///     Start playing the audio from the WaveStream
        /// </summary>
        public void Play()
        {
            if (playbackState == PlaybackState.Stopped)
            {
                playbackState = PlaybackState.Playing;
                ThreadPool.QueueUserWorkItem(state => PlaybackThread(), null);
            }
            else if (playbackState == PlaybackState.Paused)
            {
                Resume();
                callbackEvent.Set(); // give the thread a kick
            }
        }

        /// <summary>
        ///     Pause the audio
        /// </summary>
        public void Pause()
        {
            if (playbackState == PlaybackState.Playing)
            {
                MmResult result;
                lock (waveOutLock)
                {
                    result = WaveInterop.waveOutPause(hWaveOut);
                }
                if (result != MmResult.NoError)
                {
                    throw new MmException(result, "waveOutPause");
                }
                playbackState = PlaybackState.Paused;
            }
        }

        /// <summary>
        ///     Stop and reset the WaveOut device
        /// </summary>
        public void Stop()
        {
            if (playbackState != PlaybackState.Stopped)
            {
                // in the call to waveOutReset with function callbacks
                // some drivers will block here until OnDone is called
                // for every buffer
                playbackState = PlaybackState.Stopped; // set this here to avoid a problem with some drivers whereby 
                MmResult result;
                lock (waveOutLock)
                {
                    result = WaveInterop.waveOutReset(hWaveOut);
                }
                if (result != MmResult.NoError)
                {
                    throw new MmException(result, "waveOutReset");
                }
                callbackEvent.Set(); // give the thread a kick, make sure we exit
            }
        }

        /// <summary>
        ///     Playback State
        /// </summary>
        public PlaybackState PlaybackState
        {
            get { return playbackState; }
        }

        /// <summary>
        ///     Obsolete property
        /// </summary>
        [Obsolete]
        public float Volume
        {
            get { return 1.0f; }
            set { if (value != 1.0f) throw new NotImplementedException(); }
        }

        #region Dispose Pattern

        /// <summary>
        ///     Closes this WaveOut device
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        ///     Closes the WaveOut device and disposes of buffers
        /// </summary>
        /// <param name="disposing">True if called from <see>Dispose</see></param>
        protected void Dispose(bool disposing)
        {
            Stop();

            if (disposing)
            {
                DisposeBuffers();
            }

            callbackEvent.Close();
            lock (waveOutLock)
            {
                WaveInterop.waveOutClose(hWaveOut);
            }
        }

        private void DisposeBuffers()
        {
            if (buffers != null)
            {
                foreach (WaveOutBuffer buffer in buffers)
                {
                    buffer.Dispose();
                }
                buffers = null;
            }
        }

        /// <summary>
        ///     Finalizer. Only called when user forgets to call <see>Dispose</see>
        /// </summary>
        ~WaveOutEvent()
        {
            Debug.Assert(false, "WaveOutEvent device was not closed");
            Dispose(false);
        }

        #endregion

        private void PlaybackThread()
        {
            Exception exception = null;
            try
            {
                DoPlayback();
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                playbackState = PlaybackState.Stopped;
                // we're exiting our background thread
                RaisePlaybackStoppedEvent(exception);
            }
        }

        private void DoPlayback()
        {
            if (buffers == null || waveStream == null)
                return;

            TimeSpan waitTime =
                TimeSpan.FromSeconds((double) buffers[0].BufferSize/(waveStream.WaveFormat.AverageBytesPerSecond*2));
            while (playbackState != PlaybackState.Stopped)
            {
                if (callbackEvent.WaitOne())
                {
                    // requeue any buffers returned to us
                    if (playbackState == PlaybackState.Playing)
                    {
                        int queued = 0;
                        foreach (WaveOutBuffer buffer in buffers)
                        {
                            if (buffer.InQueue || buffer.OnDone())
                            {
                                queued++;
                            }
                        }
                        if (queued == 0)
                        {
                            // we got to the end
                            playbackState = PlaybackState.Stopped;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Resume playing after a pause from the same position
        /// </summary>
        private void Resume()
        {
            if (playbackState == PlaybackState.Paused)
            {
                MmResult result;
                lock (waveOutLock)
                {
                    result = WaveInterop.waveOutRestart(hWaveOut);
                }
                if (result != MmResult.NoError)
                {
                    throw new MmException(result, "waveOutRestart");
                }
                playbackState = PlaybackState.Playing;
            }
        }

        private void RaisePlaybackStoppedEvent(Exception e)
        {
            EventHandler<StoppedEventArgs> handler = PlaybackStopped;
            if (handler != null)
            {
                if (syncContext == null)
                {
                    handler(this, new StoppedEventArgs(e));
                }
                else
                {
                    syncContext.Post(state => handler(this, new StoppedEventArgs(e)), null);
                }
            }
        }
    }
}