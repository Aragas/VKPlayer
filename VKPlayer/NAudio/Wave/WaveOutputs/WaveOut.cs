using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace NAudio.Wave
{
    /// <summary>
    ///     Represents a wave out device
    /// </summary>
    public class WaveOut : IWavePlayer
    {
        private readonly WaveInterop.WaveCallback callback;
        private readonly WaveCallbackInfo callbackInfo;
        private readonly SynchronizationContext syncContext;
        private readonly object waveOutLock;
        private WaveOutBuffer[] buffers;
        private IntPtr hWaveOut;
        private volatile PlaybackState playbackState;
        private int queuedBuffers;
        private float volume = 1;
        private IWaveProvider waveStream;

        /// <summary>
        ///     Creates a default WaveOut device
        ///     Will use window callbacks if called from a GUI thread, otherwise function
        ///     callbacks
        /// </summary>
        public WaveOut()
            : this(
                SynchronizationContext.Current == null
                    ? WaveCallbackInfo.FunctionCallback()
                    : WaveCallbackInfo.NewWindow())
        {
        }

        /// <summary>
        ///     Creates a WaveOut device using the specified window handle for callbacks
        /// </summary>
        /// <param name="windowHandle">A valid window handle</param>
        public WaveOut(IntPtr windowHandle)
            : this(WaveCallbackInfo.ExistingWindow(windowHandle))
        {
        }

        /// <summary>
        ///     Opens a WaveOut device
        /// </summary>
        public WaveOut(WaveCallbackInfo callbackInfo)
        {
            syncContext = SynchronizationContext.Current;
            // set default values up
            DeviceNumber = 0;
            DesiredLatency = 300;
            NumberOfBuffers = 2;

            callback = Callback;
            waveOutLock = new object();
            this.callbackInfo = callbackInfo;
            callbackInfo.Connect(callback);
        }

        /// <summary>
        ///     Returns the number of Wave Out devices available in the system
        /// </summary>
        public static Int32 DeviceCount
        {
            get { return WaveInterop.waveOutGetNumDevs(); }
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
                result = callbackInfo.WaveOutOpen(out hWaveOut, DeviceNumber, waveStream.WaveFormat, callback);
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
                Debug.Assert(queuedBuffers == 0, "Buffers already queued on play");
                EnqueueBuffers();
            }
            else if (playbackState == PlaybackState.Paused)
            {
                EnqueueBuffers();
                Resume();
                playbackState = PlaybackState.Playing;
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

                // with function callbacks, waveOutReset will call OnDone,
                // and so PlaybackStopped must not be raised from the handler
                // we know playback has definitely stopped now, so raise callback
                if (callbackInfo.Strategy == WaveCallbackStrategy.FunctionCallback)
                {
                    RaisePlaybackStoppedEvent(null);
                }
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
        ///     Volume for this device 1.0 is full scale
        /// </summary>
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                float left = volume;
                float right = volume;

                int stereoVolume = (int) (left*0xFFFF) + ((int) (right*0xFFFF) << 16);
                MmResult result;
                lock (waveOutLock)
                {
                    result = WaveInterop.waveOutSetVolume(hWaveOut, stereoVolume);
                }
                MmException.Try(result, "waveOutSetVolume");
            }
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
                if (buffers != null)
                {
                    for (int n = 0; n < buffers.Length; n++)
                    {
                        if (buffers[n] != null)
                        {
                            buffers[n].Dispose();
                        }
                    }
                    buffers = null;
                }
            }

            lock (waveOutLock)
            {
                WaveInterop.waveOutClose(hWaveOut);
            }
            if (disposing)
            {
                callbackInfo.Disconnect();
            }
        }

        /// <summary>
        ///     Finalizer. Only called when user forgets to call <see>Dispose</see>
        /// </summary>
        ~WaveOut()
        {
            Debug.Assert(false, "WaveOut device was not closed");
            Dispose(false);
        }

        #endregion

        /// <summary>
        ///     Retrieves the capabilities of a waveOut device
        /// </summary>
        /// <param name="devNumber">Device to test</param>
        /// <returns>The WaveOut device capabilities</returns>
        public static WaveOutCapabilities GetCapabilities(int devNumber)
        {
            var caps = new WaveOutCapabilities();
            int structSize = Marshal.SizeOf(caps);
            MmException.Try(WaveInterop.waveOutGetDevCaps((IntPtr) devNumber, out caps, structSize), "waveOutGetDevCaps");
            return caps;
        }

        private void EnqueueBuffers()
        {
            for (int n = 0; n < NumberOfBuffers; n++)
            {
                if (!buffers[n].InQueue)
                {
                    if (buffers[n].OnDone())
                    {
                        Interlocked.Increment(ref queuedBuffers);
                    }
                    else
                    {
                        playbackState = PlaybackState.Stopped;
                        break;
                    }
                    //Debug.WriteLine(String.Format("Resume from Pause: Buffer [{0}] requeued", n));
                }
            }
        }

        /// <summary>
        ///     Resume playing after a pause from the same position
        /// </summary>
        public void Resume()
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

        /// <summary>
        ///     Gets the current position in bytes from the wave output device.
        ///     (n.b. this is not the same thing as the position within your reader
        ///     stream - it calls directly into waveOutGetPosition)
        /// </summary>
        /// <returns>Position in bytes</returns>
        public long GetPosition()
        {
            lock (waveOutLock)
            {
                var mmTime = new MmTime();
                mmTime.wType = MmTime.TIME_BYTES;
                // request results in bytes, TODO: perhaps make this a little more flexible and support the other types?
                MmException.Try(WaveInterop.waveOutGetPosition(hWaveOut, out mmTime, Marshal.SizeOf(mmTime)),
                    "waveOutGetPosition");

                if (mmTime.wType != MmTime.TIME_BYTES)
                    throw new Exception(string.Format("waveOutGetPosition: wType -> Expected {0}, Received {1}",
                        MmTime.TIME_BYTES, mmTime.wType));

                return mmTime.cb;
            }
        }

        // made non-static so that playing can be stopped here
        private void Callback(IntPtr hWaveOut, WaveInterop.WaveMessage uMsg, IntPtr dwInstance, WaveHeader wavhdr,
            IntPtr dwReserved)
        {
            if (uMsg == WaveInterop.WaveMessage.WaveOutDone)
            {
                var hBuffer = (GCHandle) wavhdr.userData;
                var buffer = (WaveOutBuffer) hBuffer.Target;
                Interlocked.Decrement(ref queuedBuffers);
                Exception exception = null;
                // check that we're not here through pressing stop
                if (PlaybackState == PlaybackState.Playing)
                {
                    // to avoid deadlocks in Function callback mode,
                    // we lock round this whole thing, which will include the
                    // reading from the stream.
                    // this protects us from calling waveOutReset on another 
                    // thread while a WaveOutWrite is in progress
                    lock (waveOutLock)
                    {
                        try
                        {
                            if (buffer.OnDone())
                            {
                                Interlocked.Increment(ref queuedBuffers);
                            }
                        }
                        catch (Exception e)
                        {
                            // one likely cause is soundcard being unplugged
                            exception = e;
                        }
                    }
                }
                if (queuedBuffers == 0)
                {
                    if (callbackInfo.Strategy == WaveCallbackStrategy.FunctionCallback &&
                        playbackState == PlaybackState.Stopped)
                    {
                        // the user has pressed stop
                        // DO NOT raise the playback stopped event from here
                        // since on the main thread we are still in the waveOutReset function
                        // Playback stopped will be raised elsewhere
                    }
                    else
                    {
                        RaisePlaybackStoppedEvent(exception);
                    }
                }
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