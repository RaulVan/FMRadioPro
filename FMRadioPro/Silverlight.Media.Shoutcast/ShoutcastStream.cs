//-----------------------------------------------------------------------
// <copyright file="ShoutcastStream.cs" company="Andrew Oakley">
//     Copyright (c) 2010 Andrew Oakley
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public License
//     along with this program.  If not, see http://www.gnu.org/licenses.
// </copyright>
//-----------------------------------------------------------------------

namespace Silverlight.Media
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using Silverlight.Media.Extensions;
    using Silverlight.Media.Metadata;
    using Silverlight.Media.Parsers;

    /// <summary>
    /// Implements the Shoutcast streaming protocol.
    /// </summary>
    public class ShoutcastStream : Stream
    {
        /// <summary>
        /// The default initial buffer size.
        /// </summary>
        private const int DefaultInitialBufferSize = 8192;

        /// <summary>
        /// Number of milliseconds to sleep if a buffer read will overwrite our read buffer's pointer.
        /// </summary>
        private const int BufferOverwriteSleepTime = 10;

        /// <summary>
        /// Number of seconds per frame, per MP3 specification.
        /// </summary>
        private const double NumberOfSecondsPerMp3Frame = 0.026d;

        /// <summary>
        /// Default number of seconds to buffer.
        /// </summary>
        private const int DefaultSecondsToBuffer = 10;

        /// <summary>
        /// Number of times to retry reading from the initial buffer read.
        /// </summary>
        private const int NumberOfReadRetries = 10;

        /// <summary>
        /// Minimum number of bytes required to keep in the buffer.
        /// </summary>
        private int minimumBufferedBytes;

        /// <summary>
        /// Number of seconds to buffer.
        /// </summary>
        private int numberOfSecondsToBuffer;

        /// <summary>
        /// Background worker to fill circular buffer from network stream.
        /// </summary>
        private BackgroundWorker backgroundWorker;

        /// <summary>
        /// Shoutcast metadata interval byte count.
        /// </summary>
        private int icyMetadata;

        /// <summary>
        /// Inner stream providing MP3 bytes.
        /// </summary>
        private Stream innerStream;

        /// <summary>
        /// Current stream metadata.
        /// </summary>
        private string currentMetadata;

        /// <summary>
        /// Current parsed stream metadata.
        /// </summary>
        private ShoutcastMetadata currentMpegMetadata = new ShoutcastMetadata();

        /// <summary>
        /// Circular buffer synchronization object.
        /// </summary>
        private object syncRoot = new object();

        /// <summary>
        /// Number of bytes left in stream until metadata.
        /// </summary>
        private int metadataCount;

        /// <summary>
        /// Audio stream description.
        /// </summary>
        private MediaStreamDescription audioStreamDescription;

        /// <summary>
        /// Audio source attributes.
        /// </summary>
        private Dictionary<MediaSourceAttributesKeys, string> audioSourceAttributes;

        /// <summary>
        /// Current frame size.
        /// </summary>
        private int currentFrameSize;

        /// <summary>
        /// Circular buffer for audio stream data.
        /// </summary>
        private CircularBuffer<byte> circularBuffer;

        /// <summary>
        /// Number of bytes left in current frame.
        /// </summary>
        private int bytesLeftInFrame;

        /// <summary>
        /// MpegFrame representing the next MP3 frame.
        /// </summary>
        private AudioFrame nextFrame;

        /// <summary>
        /// MpegLayer3WaveFormat representing MP3 format.
        /// </summary>
        private WaveFormat mpegLayer3WaveFormat;

        /// <summary>
        /// Current percentage of the minimum required bytes in the circular buffer.
        /// </summary>
        private int bufferingPercentage;

        /// <summary>
        /// MediaStreamSource associated with this stream.
        /// </summary>
        private ShoutcastMediaStreamSource mediaStreamSource;

        /// <summary>
        /// Represents whether or not this stream should request to receive metadata.
        /// </summary>
        private bool includeMetadata;

        /// <summary>
        /// Text encoding used to decode metadata bytes.
        /// </summary>
        private Encoding metadataEncoding;

        /// <summary>
        /// Initializes a new instance of the ShoutcastStream class.
        /// </summary>
        /// <param name="mediaStreamSource">ShoutcastMediaStreamSource containing this ShoutcastStream.</param>
        /// <param name="httpWebResponse">HttpWebResponse for MP3 stream request.</param>
        public ShoutcastStream(ShoutcastMediaStreamSource mediaStreamSource, HttpWebResponse httpWebResponse)
            : this(mediaStreamSource, httpWebResponse, ShoutcastStream.DefaultSecondsToBuffer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ShoutcastStream class.
        /// </summary>
        /// <param name="mediaStreamSource">ShoutcastMediaStreamSource containing this ShoutcastStream.</param>
        /// <param name="httpWebResponse">HttpWebResponse for MP3 stream request.</param>
        /// <param name="numberOfSecondsToBuffer">Number of seconds of audio data to buffer.</param>
        public ShoutcastStream(ShoutcastMediaStreamSource mediaStreamSource, HttpWebResponse httpWebResponse, int numberOfSecondsToBuffer)
            : this(mediaStreamSource, httpWebResponse, numberOfSecondsToBuffer, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ShoutcastStream class.
        /// </summary>
        /// <param name="mediaStreamSource">ShoutcastMediaStreamSource containing this ShoutcastStream.</param>
        /// <param name="httpWebResponse">HttpWebResponse for MP3 stream request.</param>
        /// <param name="numberOfSecondsToBuffer">Number of seconds of audio data to buffer.</param>
        /// <param name="metadataEncoding">Text encoding used to decode the Shoutcast metadata.</param>
        public ShoutcastStream(ShoutcastMediaStreamSource mediaStreamSource, HttpWebResponse httpWebResponse, int numberOfSecondsToBuffer, Encoding metadataEncoding)
        {
            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  ShoutcastStream() ctor");
            if (mediaStreamSource == null)
            {
                throw new ArgumentNullException("mediaStreamSource");
            }

            if (httpWebResponse == null)
            {
                throw new ArgumentNullException("httpWebResponse");
            }

            if (metadataEncoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            this.mediaStreamSource = mediaStreamSource;
            this.includeMetadata = this.mediaStreamSource.IncludeMetadata;
            this.numberOfSecondsToBuffer = numberOfSecondsToBuffer;
            this.metadataEncoding = metadataEncoding;

            // If the request is bad, this will die first, so we can just not worry about it.
            this.innerStream = httpWebResponse.GetResponseStream();
            try
            {
                // Read a chunk of data, but likely one smaller than the circular buffer size.
                byte[] initialBuffer = new byte[ShoutcastStream.DefaultInitialBufferSize];

                // Make sure we have enough data to work with initially
                int bytesRead = this.ForceReadFromStream(initialBuffer, 0, initialBuffer.Length);

                if (bytesRead == 0)
                {
                    // Should this be -1?
                    // This means there was something wrong with the stream.
                    throw new InvalidOperationException("Zero initial bytes read from stream.");
                }

                this.MediaInformation = this.FindStreamInformation(httpWebResponse, ref initialBuffer);

                if (this.MediaInformation == null)
                {
                    throw new ArgumentException("Invalid MediaInformation");
                }

                this.icyMetadata = this.MediaInformation.MetadataInterval;
                this.ParseInitialBuffer(initialBuffer);
            }
            catch (Exception)
            {
                // No matter what, we need to shut down!
                if (this.innerStream != null)
                {
                    this.innerStream.Dispose();
                    this.innerStream = null;
                }

                // Rethrow
                throw;
            }

            this.backgroundWorker = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };

            this.backgroundWorker.DoWork += new DoWorkEventHandler(this.DoWork);
            this.backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.RunWorkerCompleted);
            this.backgroundWorker.RunWorkerAsync(this.innerStream);
        }

        /// <summary>
        /// Called after the ShoutcastStream has been completely shut down.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the value containing the current stream information.
        /// </summary>
        public ShoutcastStreamInformation MediaInformation { get; private set; }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "This property is overridden from the base class, so we have to throw this exception.")]
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "This property is overridden from the base class, so we have to throw this exception.")]
        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value containing the audio stream description.
        /// </summary>
        public MediaStreamDescription AudioStreamDescription
        {
            get { return this.audioStreamDescription; }
        }

        /// <summary>
        /// Gets a value containing the audio source attributes.
        /// </summary>
        public Dictionary<MediaSourceAttributesKeys, string> AudioSourceAttributes
        {
            get { return this.audioSourceAttributes; }
        }

        /// <summary>
        /// Gets a value representing the current MP3 frame size.
        /// </summary>
        public int CurrentFrameSize
        {
            get { return this.currentFrameSize; }
        }

        /// <summary>
        /// Gets a value representing the current MP3 wave format.
        /// </summary>
        public WaveFormat WaveFormat
        {
            get { return this.mpegLayer3WaveFormat; }
        }

        /// <summary>
        /// Gets a value representing the current percentage of the minimum required bytes in the circular buffer.
        /// </summary>
        public int BufferingPercentage
        {
            get { return this.bufferingPercentage; }
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        public override int ReadByte()
        {
            // Nobody should use this, as it complicates the metadata stuff, so let's just throw for now.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            this.bytesLeftInFrame -= count;

            this.ReadOrPeekBuffer(buffer, count, false);
            if (this.bytesLeftInFrame == 0)
            {
                this.SetupNextFrame();
            }

            return count;
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Stream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Dispose(bool)");
            if (disposing)
            {
                System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Dispose(true)");
                this.backgroundWorker.CancelAsync();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes a WaveFormatExtensible instance representing an AAC+ frame.
        /// </summary>
        /// <param name="audioFrame">Audio frame representing an AAC+ frame.</param>
        /// <returns>A WaveFormatExtensible for the supplied audio frame.</returns>
        private static HeAacWaveFormat CreateAacPlusFormat(AudioFrame audioFrame)
        {
            if (audioFrame == null)
            {
                throw new ArgumentNullException("audioFrame");
            }

            WaveFormatExtensible wfx = new WaveFormatExtensible();

            wfx.FormatTag = 0x1610;
            wfx.Channels = (short)audioFrame.NumberOfChannels;
            wfx.SamplesPerSec = audioFrame.SamplingRate;
            wfx.AverageBytesPerSecond = audioFrame.BitRate / 8;
            wfx.BlockAlign = 1;
            wfx.BitsPerSample = 0;
            wfx.Size = 12;

            HeAacWaveFormat aacf = new HeAacWaveFormat(wfx);

            // Extra 3 words in WAVEFORMATEX
            aacf.PayloadType = 0x1; // Audio Data Transport Stream (ADTS). The stream contains an adts_sequence, as defined by MPEG-2.
            aacf.AudioProfileLevelIndication = 0xFE;
            aacf.StructType = 0;

            return aacf;
        }

        /// <summary>
        /// Initializes a WaveFormatExtensible instance representing an MP3 frame.
        /// </summary>
        /// <param name="audioFrame">Audio frame representing an MP3 frame.</param>
        /// <returns>A WaveFormatExtensible for the supplied audio frame.</returns>
        private static MpegLayer3WaveFormat CreateMp3WaveFormat(AudioFrame audioFrame)
        {
            if (audioFrame == null)
            {
                throw new ArgumentNullException("audioFrame");
            }

            WaveFormatExtensible waveFormatExtensible = new WaveFormatExtensible()
            {
                AverageBytesPerSecond = audioFrame.BitRate / 8,
                BitsPerSample = 0,
                BlockAlign = 1,
                Channels = (short)audioFrame.NumberOfChannels,
                FormatTag = 85,
                SamplesPerSec = audioFrame.SamplingRate,
                Size = 12
            };

            MpegLayer3WaveFormat waveFormat = new MpegLayer3WaveFormat(waveFormatExtensible);
            waveFormat.Id = 1;
            waveFormat.BitratePaddingMode = 0;
            waveFormat.FramesPerBlock = 1;
            waveFormat.BlockSize = (short)audioFrame.FrameSize;
            waveFormat.CodecDelay = 0;

            return waveFormat;
        }

        /// <summary>
        /// Reads or Peeks data from the circular buffer.
        /// </summary>
        /// <param name="buffer">Buffer in which to put the read or peeked data.</param>
        /// <param name="count">Number of bytes to read or peek.</param>
        /// <param name="shouldPeek">true if the data should be peeked from the circular buffer, otherwise the data is read from the circular buffer.</param>
        private void ReadOrPeekBuffer(byte[] buffer, int count, bool shouldPeek)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (this.ReadIncludesMetadata(count))
            {
                // Read to metadata chunk
                int metadataOffset = this.icyMetadata - this.metadataCount;
                byte[] metadataSizeBuffer = new byte[metadataOffset + 1];
                int bytesRead = 0;
                int metadataSize = 0;
                lock (this.syncRoot)
                {
                    bytesRead = this.circularBuffer.Peek(metadataSizeBuffer, 0, metadataOffset + 1);
                }

                if (bytesRead != (metadataOffset + 1))
                {
                    // We didn't read enough.
                    throw new IndexOutOfRangeException("metadataSize buffer not filled.");
                }

                metadataSize = metadataSizeBuffer[metadataOffset];
                int metadataByteCount = metadataSize * 16;
                int numberOfBytesAfterMetadata = count - metadataOffset;

                // We need the size of the pre-metadata bytes + metadata size byte + metadata byte count + remaining data bytes.
                byte[] metadataBuffer = new byte[metadataOffset + 1 + metadataByteCount + numberOfBytesAfterMetadata];

                lock (this.syncRoot)
                {
                    // Here is where we either Get() or Peek().  The work before is just to get the right size.
                    if (shouldPeek)
                    {
                        bytesRead = this.circularBuffer.Peek(metadataBuffer, 0, metadataBuffer.Length);
                    }
                    else
                    {
                        bytesRead = this.circularBuffer.Get(metadataBuffer, 0, metadataBuffer.Length);
                        this.metadataCount = numberOfBytesAfterMetadata;
                    }
                }

                // We are going to throw the metadata away here, as it will be read again.
                // Copy from beginning to metadata offset
                Array.Copy(metadataBuffer, 0, buffer, 0, metadataOffset);

                // Copy after metadata
                Array.Copy(metadataBuffer, metadataOffset + 1 + metadataByteCount, buffer, metadataOffset, numberOfBytesAfterMetadata);

                // Only change the metadata when we ACTUALLY read.
                if ((!shouldPeek) && (metadataSize != 0))
                {
                    string newMetadata = this.metadataEncoding.GetString(metadataBuffer, metadataOffset + 1, metadataByteCount) ?? string.Empty;

                    // TODO - Should we fire this every time the metadata changes, or whenever it is parsed.
                    // See if we need to fire the metadata changed event
                    if (string.Compare(this.currentMetadata, newMetadata) != 0)
                    {
                        this.currentMetadata = newMetadata;

                        // We need to set the current metadata on the MSS so it is always available.
                        ShoutcastMetadata metadata = new ShoutcastMetadata(this.currentMetadata);
                        Interlocked.Exchange<ShoutcastMetadata>(ref this.mediaStreamSource.currentMetadata, metadata);

                        // Since MediaElement can only be created on the UI thread, we will marshal this event over to the UI thread, plus this keeps us from blocking.
                        Deployment.Current.Dispatcher.BeginInvoke(() => this.mediaStreamSource.OnMetadataChanged());
                    }
                }
            }
            else
            {
                int bytesRead;
                lock (this.syncRoot)
                {
                    // Here is where we either Get() or Peek().  The work before is just to get the right size.
                    if (shouldPeek)
                    {
                        bytesRead = this.circularBuffer.Peek(buffer, 0, count);
                    }
                    else
                    {
                        bytesRead = this.circularBuffer.Get(buffer, 0, count);
                        this.metadataCount += bytesRead;
                    }
                }
            }
        }

        /// <summary>
        /// Parses initial audio stream byte buffer.
        /// </summary>
        /// <param name="initialBuffer">Initial bytes from the audio stream.</param>
        private void ParseInitialBuffer(byte[] initialBuffer)
        {
            // Initialize data structures to pass to the Media pipeline via the MediaStreamSource
            Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();

            byte[] audioData = initialBuffer;
            int bytesRead = initialBuffer.Length;

            AudioFrame mpegLayer3Frame;
            int result = this.SyncStream(audioData, out mpegLayer3Frame);
            this.metadataCount = result;

            if (this.MediaInformation.ContentType == "audio/mpeg")
            {
                this.mpegLayer3WaveFormat = ShoutcastStream.CreateMp3WaveFormat(mpegLayer3Frame);
            }
            else if (this.MediaInformation.ContentType == "audio/aacp")
            {
                this.mpegLayer3WaveFormat = ShoutcastStream.CreateAacPlusFormat(mpegLayer3Frame);
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid content type: {0}", this.MediaInformation.ContentType));
            }

            mediaStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = this.mpegLayer3WaveFormat.ToHexString();

            this.audioStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, mediaStreamAttributes);

            // Setting a 0 duration, since we are a potentially infinite Mp3 stream.
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromMinutes(0).Ticks.ToString(CultureInfo.InvariantCulture);

            // No seeking within the stream!
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = "0";

            this.audioSourceAttributes = mediaSourceAttributes;

            this.currentFrameSize = mpegLayer3Frame.FrameSize;
            
            // Set up bytes left in frame so we can support non-frame size counts
            this.bytesLeftInFrame = this.currentFrameSize;
            this.nextFrame = mpegLayer3Frame;

            int bufferByteSize = this.CalculateCircularBufferSize(mpegLayer3Frame.FrameSize);
            this.circularBuffer = new CircularBuffer<byte>(bufferByteSize, true);
            this.circularBuffer.Put(audioData, result, bytesRead - result);

            // Read some more to fill out the buffer
            // audioData = new byte[this.minimumBufferedBytes - this.circularBuffer.Size];

            // We have to force reading from the stream at first.  This is because when we read from the NetworkStream, it will return all of the available data in its buffer.
            // If there is less data than we ask for, it only returns what it has.  It does not block until it has enough.  So, we will force a loop until we have read what we need.
            // bytesRead = this.ForceReadFromStream(audioData, 0, audioData.Length);
            // this.circularBuffer.Put(audioData, 0, bytesRead);
        }

        /// <summary>
        /// Forces the specified number of bytes to be read from the inner stream.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This should be the same as the count parameter.</returns>
        private int ForceReadFromStream(byte[] buffer, int offset, int count)
        {
            // NOTE - We aren't locking the inner NetworkStream here.  This is because we don't need to yet, as the thread loop hasn't started.
            int bytesRead = 0;
            int loopCount = 0;
            int zeroReadCount = 0;
            int tempBytesRead = 0;

            // We need to be careful here.  If for some reason the stream stops, this will be stuck in an infinite loop.  So we'll put some insurance in here so we can't get stuck.
            while (bytesRead != count)
            {
                tempBytesRead = this.innerStream.Read(buffer, offset + bytesRead, count - bytesRead);
                if (tempBytesRead == 0)
                {
                    // Rest for a tenth of a second and try again.  If we do this ten times, bail so we don't get stuck.
                    zeroReadCount++;
                    if (zeroReadCount == ShoutcastStream.NumberOfReadRetries)
                    {
                        return 0;
                    }

                    Thread.Sleep(100);
                }
                else
                {
                    // Reset zeroReadCount as we have SOME data.
                    zeroReadCount = 0;
                    bytesRead += tempBytesRead;
                }

                loopCount++;
            }

            return bytesRead;
        }

        /// <summary>
        /// Calculates the buffer size required for this audio stream.
        /// </summary>
        /// <param name="initialFrameSize">Size, in bytes, of the initial MP3 frame.</param>
        /// <returns>The required size of the circular buffer.</returns>
        private int CalculateCircularBufferSize(int initialFrameSize)
        {
            // Number of frames per second, rounded up to whole number
            int numberOfFramesPerSecond = (int)Math.Ceiling(1.0 / ShoutcastStream.NumberOfSecondsPerMp3Frame);

            // Number of bytes needed to buffer n seconds, rounded up to the nearest power of 2.  This defaults to 10 seconds.
            this.minimumBufferedBytes = (int)Math.Pow(2, Math.Ceiling(Math.Log(this.numberOfSecondsToBuffer * numberOfFramesPerSecond * initialFrameSize) / Math.Log(2)));

            // Return the circular buffer size, which is our minimum buffered bytes * 2, so we have the capacity of n * 2 number of seconds in our buffer
            return this.minimumBufferedBytes * 2;
        }

        /// <summary>
        /// Method used by the BackgroundWorker to read audio data from the inner stream.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A DoWorkEventArgs that contains the event data.</param>
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                Stream stream = (Stream)e.Argument;
                int bufferLength = this.icyMetadata > 0 ? this.icyMetadata : ShoutcastStream.DefaultInitialBufferSize;
                byte[] buffer = new byte[bufferLength];
                int bytesRead = 0;
                int availableBytes;
                int bufferingPercent;
                using (ManualResetEvent shutdownEvent = new ManualResetEvent(false))
                {
                    while (true)
                    {
                        System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Work loop start");
                        if (worker.CancellationPending)
                        {
                            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Cancellation");
                            e.Cancel = true;
                            shutdownEvent.Set();
                            break;
                        }

                        System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  No cancellation pending");
                        lock (this.syncRoot)
                        {
                            availableBytes = this.circularBuffer.Capacity - this.circularBuffer.Size;

                            // This is for reporting buffer progress, if needed.
                            bufferingPercent = Math.Min((int)(((double)this.circularBuffer.Size / (double)this.minimumBufferedBytes) * 100), 100);
                        }

                        // Update the buffering percentage, if needed.
                        if (bufferingPercent != this.bufferingPercentage)
                        {
                            // The current buffering percent has fallen below the previous percentage, so replace.
                            // We have to do a little math voodoo since Silverlight doesn't support exchanging doubles.
                            Interlocked.Exchange(ref this.bufferingPercentage, bufferingPercent);
                        }

                        if (availableBytes < bufferLength)
                        {
                            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Not enough bytes available.  Sleeping.");

                            // We'll overwrite the head pointer, so sleep, and reloop.
                            shutdownEvent.WaitOne(ShoutcastStream.BufferOverwriteSleepTime);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Reading from stream.");
                            bytesRead = stream.Read(buffer, 0, bufferLength);
                            if (bytesRead > 0)
                            {
                                lock (this.syncRoot)
                                {
                                    this.circularBuffer.Put(buffer, 0, bytesRead);
                                }
                            }

                            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Done reading from stream.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Normally, this is going to be the ThreadAbortException, which happens if the CLR is shutdown before we are closed.
                // Call the completed method ourselves.
                System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  DoWork() - Exception:  {0}", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Parses the Shoutcast specific headers.  This method is different because of how Shoutcast responds to an HttpWebRequest.  The headers need to be parsed, then removed from the initialBuffer.
        /// </summary>
        /// <param name="initialBuffer">Initial data buffer from the audio stream.</param>
        /// <returns>ShoutcastStreamInformation containing information about the audio stream.</returns>
        private ShoutcastStreamInformation ParseShoutcastHeaders(ref byte[] initialBuffer)
        {
            ShoutcastStreamInformation result = null;
            int byteCount = 0;
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // We may have a REAL ICY stream
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(initialBuffer, false);
                using (StreamReader reader = new StreamReader(stream, this.metadataEncoding, true))
                {
                    // This is to resolve CA2202.
                    stream = null;

                    // Read until we get a blank line.  This is SUCH a bad and unsafe way to parse "http" headers.
                    List<string> headerLines = new List<string>();
                    string line;
                    string responseHeader;
                    HttpStatusCode status = HttpStatusCode.NotFound;
                    string statusDescription = string.Empty;

                    // Get the ICY header
                    responseHeader = reader.ReadLine();
                    string[] headerParts = responseHeader.Split(' ');
                    if (headerParts.Length >= 2)
                    {
                        string s = headerParts[1];
                        status = (HttpStatusCode)int.Parse(s);
                        if (headerParts.Length >= 3)
                        {
                            string str3 = headerParts[2];
                            for (int i = 3; i < headerParts.Length; i++)
                            {
                                str3 = str3 + " " + headerParts[i];
                            }

                            statusDescription = str3;
                        }
                    }

                    if (status != HttpStatusCode.OK)
                    {
                        // Bail!
                        return result;
                    }

                    byteCount = responseHeader.Length + 2;
                    while (!string.IsNullOrEmpty((line = reader.ReadLine())))
                    {
                        headerLines.Add(line);
                    }

                    // We should be pointing right at the data now! :)
                    // Parse the headers
                    foreach (string headerLine in headerLines)
                    {
                        byteCount += this.metadataEncoding.GetByteCount(headerLine) + 2;
                        int colonIndex = headerLine.IndexOf(':');
                        string key = headerLine.Substring(0, colonIndex);
                        string value = headerLine.Substring(colonIndex + 1).Trim();

                        // We are going to not duplicate headers for now, as this requires order parsing, comma appending, etc.
                        // http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                        if (!responseHeaders.ContainsKey(key))
                        {
                            responseHeaders.Add(key, value);
                        }
                    }

                    // Add the last CRLF
                    byteCount += 2;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            // Resize the initialBuffer to reflect the headers we've read.
            int newBufferLength = initialBuffer.Length - byteCount;
            byte[] tempBuffer = initialBuffer;
            initialBuffer = new byte[newBufferLength];
            Array.Copy(tempBuffer, byteCount, initialBuffer, 0, newBufferLength);

            result = new ShoutcastStreamInformation(responseHeaders);

            if (result.MetadataInterval == -1)
            {
                // TODO - Fix this!!!
                return null;
            }

            return result;
        }

        /// <summary>
        /// Parses the headers from the audio stream.
        /// </summary>
        /// <param name="httpWebResponse">HttpWebResponse from the server sending the audio stream.</param>
        /// <param name="initialBuffer">Initial data buffer from the audio stream.</param>
        /// <returns>ShoutcastStreamInformation containing information about the audio stream.</returns>
        private ShoutcastStreamInformation FindStreamInformation(HttpWebResponse httpWebResponse, ref byte[] initialBuffer)
        {
            if (httpWebResponse == null)
            {
                throw new ArgumentNullException("httpWebResponse");
            }

            if (initialBuffer == null)
            {
                throw new ArgumentNullException("initialBuffer");
            }

            ShoutcastStreamInformation result = null;

            // See if we are a Shoutcast stream.
            if (string.IsNullOrEmpty(httpWebResponse.Headers[HttpRequestHeader.ContentType]))
            {
                // We may have a REAL ICY stream
                result = this.ParseShoutcastHeaders(ref initialBuffer);
            }
            else
            {
                // We are a non-Shoutcast server stream, so we can assign the information here.
                result = new ShoutcastStreamInformation(httpWebResponse.Headers.ToDictionary());
            }

            return result;
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the background worker.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A RunWorkerCompletedEventArgs that contains the event data.</param>
        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  RunWorkerCompleted()");
            this.innerStream.Close();
            if (e.Error != null)
            {
                Interlocked.Exchange<Exception>(ref this.mediaStreamSource.workerException, e.Error);
            }

            var handler = this.Closed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Indicates if reading the specified number of bytes from the circular buffer contains MP3 metadata.
        /// </summary>
        /// <param name="count">Number of bytes to read from the circular buffer.</param>
        /// <returns>true if reading the specified number of bytes from the circular buffer contains MP3 metadata, otherwise, false.</returns>
        private bool ReadIncludesMetadata(int count)
        {
            return this.includeMetadata && ((this.metadataCount + count) >= this.icyMetadata);
        }

        /// <summary>
        /// Synchronizes the MP3 data on a frame header.
        /// </summary>
        /// <param name="audioData">Byte array representing a chunk of MP3 data.</param>
        /// <param name="mpegFrame">Assigned to the resultant, parsed MpegFrame pointed to by the return value.</param>
        /// <returns>Offset into the audioData parameters representing the next, valid MpegFrame</returns>
        private int SyncStream(byte[] audioData, out AudioFrame mpegFrame)
        {
            if (audioData == null)
            {
                throw new ArgumentNullException("audioData");
            }

            if (audioData.Length == 0)
            {
                throw new ArgumentException("audioData cannot have a Length of 0.");
            }

            int frameHeaderSize;
            byte[] syncBytes;
            Func<byte[], bool> isValidFrame;
            Func<byte[], AudioFrame> createFrame;

            if (this.MediaInformation.ContentType == "audio/mpeg")
            {
                frameHeaderSize = MpegFrame.FrameHeaderSize;
                syncBytes = MpegFrame.SyncBytes;
                isValidFrame = MpegFrame.IsValidFrame;
                createFrame = b => new MpegFrame(b);
            }
            else if (this.MediaInformation.ContentType == "audio/aacp")
            {
                frameHeaderSize = AacpFrame.FrameHeaderSize;
                syncBytes = AacpFrame.SyncBytes;
                isValidFrame = AacpFrame.IsValidFrame;
                createFrame = b => new AacpFrame(b);
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid content type: {0}", this.MediaInformation.ContentType));
            }

            // We need to restructure this whole thing!!!
            // This is PROBABLY due to an intro file, so resync and hope for the best. :D
            int bytesRead = audioData.Length;

            // Find the syncpoint
            int result = BitTools.FindBitPattern(audioData, syncBytes, syncBytes);
            AudioFrame mpegLayer3Frame = null;
            byte[] frameHeader = new byte[frameHeaderSize];

            // TODO - Make sure we have enough left in the array, otherwise, we'll get an exception!
            while (mpegLayer3Frame == null)
            {
                if (result == -1)
                {
                    // Something is wrong.  Likely due to the the socket returning no data.
                    // We'll throw for now.
                    throw new InvalidOperationException("Sync bit pattern not found");
                }

                Array.Copy(audioData, result, frameHeader, 0, frameHeaderSize);

                if (isValidFrame(frameHeader))
                {
                    mpegLayer3Frame = createFrame(frameHeader);

                    // If this works, we need to take the frame size, index into the buffer, and pull out another frame header.  If the sample rate and such match, then we are good.
                    // Otherwise, we need to find the next set of sync bytes starting at the first index and do it again.  This is to reduce the false positives.
                    byte[] nextFrameHeader = new byte[frameHeaderSize];
                    Array.Copy(audioData, result + mpegLayer3Frame.FrameSize, nextFrameHeader, 0, frameHeaderSize);
                    if (isValidFrame(nextFrameHeader))
                    {
                        // Both are valid frame, so compare.
                        AudioFrame nextMpegLayer3Frame = createFrame(nextFrameHeader);

                        // Check the version, layer, sampling frequency, and number of channels.  If they match, we should be good.
                        if (!mpegLayer3Frame.Equals(nextMpegLayer3Frame))
                        {
                            mpegLayer3Frame = null;
                            result = BitTools.FindBitPattern(audioData, syncBytes, syncBytes, result + 1);
                        }
                    }
                    else
                    {
                        // The second frame header was not valid, so we need to reset.
                        mpegLayer3Frame = null;
                        result = BitTools.FindBitPattern(audioData, syncBytes, syncBytes, result + 1);
                    }
                }
                else
                {
                    // The second frame header was not valid, so we need to reset.
                    mpegLayer3Frame = null;
                    result = BitTools.FindBitPattern(audioData, syncBytes, syncBytes, result + 1);
                }
            }

            mpegFrame = mpegLayer3Frame;
            return result;
        }

        /// <summary>
        /// Resynchronizes the audio stream to the next valid Mp3 frame.
        /// </summary>
        private void ResyncStream()
        {
            // We need to restructure this whole thing!!!
            // This is PROBABLY due to an intro file, so resync and hope for the best. :D
            byte[] audioData = new byte[ShoutcastStream.DefaultInitialBufferSize];
            int bytesRead;
            lock (this.syncRoot)
            {
                bytesRead = this.circularBuffer.Peek(audioData, 0, audioData.Length);
            }

            AudioFrame mpegLayer3Frame;
            int result = this.SyncStream(audioData, out mpegLayer3Frame);

            // Throw away X bytes
            byte[] garbage = new byte[result];
            bytesRead = this.circularBuffer.Get(garbage, 0, result);

            // Fix the metadata
            this.metadataCount += bytesRead;
            this.currentFrameSize = mpegLayer3Frame.FrameSize;
            this.bytesLeftInFrame = this.currentFrameSize;
            this.nextFrame = mpegLayer3Frame;
        }

        /// <summary>
        /// Reads the next MP3 frame header.
        /// </summary>
        private void SetupNextFrame()
        {
            int frameHeaderSize;
            byte[] frameHeader;
            Func<byte[], bool> isValidFrame;
            Func<byte[], AudioFrame> createFrame;

            if (this.MediaInformation.ContentType == "audio/mpeg")
            {
                frameHeaderSize = MpegFrame.FrameHeaderSize;
                frameHeader = new byte[frameHeaderSize];
                isValidFrame = MpegFrame.IsValidFrame;
                createFrame = b => new MpegFrame(b);
            }
            else if (this.MediaInformation.ContentType == "audio/aacp")
            {
                frameHeaderSize = AacpFrame.FrameHeaderSize;
                frameHeader = new byte[frameHeaderSize];
                isValidFrame = AacpFrame.IsValidFrame;
                createFrame = b => new AacpFrame(b);
            }
            else
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid content type: {0}", this.MediaInformation.ContentType));
            }

            // If bytesLeftInFrame == 0, then we need to read the next frame header
            if (this.bytesLeftInFrame == 0)
            {
                this.ReadOrPeekBuffer(frameHeader, frameHeaderSize, true);

                if (isValidFrame(frameHeader))
                {
                    AudioFrame mpegFrame = createFrame(frameHeader);
                    this.currentFrameSize = mpegFrame.FrameSize;
                    this.bytesLeftInFrame = this.currentFrameSize;
                    this.nextFrame = mpegFrame;
                }
                else
                {
                    // We are out of sync, probably due to an intro
                    this.ResyncStream();
                }
            }
        }
    }
}
