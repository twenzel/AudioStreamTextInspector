using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace AudioStreamTextInspector.Internal
{
    /// <summary>
    /// Slices the mp3 stream into wav packages
    /// </summary>
    public class StreamSlicer
    {
        private bool _continueSlicing;
        private readonly string _audioStreamUrl;
        private int _sliceDuration = 20;

        /// <summary>
        /// Will be raised whenever a new slice is ready
        /// </summary>
        public event EventHandler<MemoryStream> OnNewSlice;

        /// <summary>
        /// Will be raised when the stream ends
        /// </summary>
        public event EventHandler<EventArgs> OnStreamEnd;

        /// <summary>
        /// Will be raised on stream exceptions
        /// </summary>
        public event EventHandler<StreamErrorEventArgs> OnStreamError;

        /// <summary>
        /// Gets or sets the duration of each slice in seconds
        /// </summary>
        public int SliceDuration
        {
            get => _sliceDuration;
            set => _sliceDuration = value > 0 ? value : throw new ArgumentException("Value should be a non-negativ number");
        }

        /// <summary>
        /// Initializes the slicer instance
        /// </summary>
        /// <param name="audioStreamUrl">Url of the audio stream</param>
        public StreamSlicer(string audioStreamUrl)
        {
            if (string.IsNullOrEmpty(audioStreamUrl))
                throw new ArgumentNullException("Please provide an url to the stream", nameof(audioStreamUrl));

            _audioStreamUrl = audioStreamUrl;
        }

        /// <summary>
        /// Starts the slicing
        /// </summary>
        public void Start()
        {
            IMp3FrameDecompressor decompressor = null;
            MemoryStream currentSlice = null;
            var buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame            
            WaveFileWriter writer = null;

            _continueSlicing = true;

            try
            {
                using (var responseStream = WebRequest.Create(_audioStreamUrl).GetResponse().GetResponseStream())
                {
                    using (var readFullyStream = new ReadFullyStream(responseStream))
                    {
                        do
                        {
                            Mp3Frame frame;
                            try
                            {
                                frame = Mp3Frame.LoadFromStream(readFullyStream);
                            }
                            catch (EndOfStreamException)
                            {
                                OnStreamEnd?.Invoke(this, EventArgs.Empty);
                                break;
                            }
                            catch (WebException e)
                            {
                                var args = new StreamErrorEventArgs(e);
                                OnStreamError?.Invoke(this, args);

                                if (!args.Handled)
                                    throw;

                                break;
                            }

                            if (decompressor == null)
                                decompressor = CreateFrameDecompressor(frame);

                            int decompressed = decompressor.DecompressFrame(frame, buffer, 0);


                            if (currentSlice == null)
                            {
                                currentSlice = new MemoryStream();
                                writer = new WaveFileWriter(currentSlice, decompressor.OutputFormat);
                            }

                            writer.Write(buffer, 0, decompressed);

                            if (currentSlice.Length > decompressor.OutputFormat.AverageBytesPerSecond * SliceDuration) // 20 sec
                            {
                                writer.Flush();

                                OnNewSlice?.Invoke(this, new MemoryStream(currentSlice.ToArray()));

                                writer.Dispose();
                                currentSlice = null;
                            }


                        } while (_continueSlicing);

                        // was doing this in a finally block, but for some reason
                        // we are hanging on response stream .Dispose so never get there
                        decompressor.Dispose();
                    }
                }
            }
            finally
            {
                decompressor?.Dispose();
                writer?.Dispose();
                currentSlice?.Dispose();
            }
        }

        /// <summary>
        /// Stops the slicing
        /// </summary>
        public void Stop()
        {
            _continueSlicing = false;
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

    }
}
