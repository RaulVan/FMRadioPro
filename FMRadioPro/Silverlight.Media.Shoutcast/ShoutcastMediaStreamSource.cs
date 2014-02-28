//-----------------------------------------------------------------------
// <copyright file="ShoutcastMediaStreamSource.cs" company="Andrew Oakley">
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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
    "CA1709:IdentifiersShouldBeCasedCorrectly",
    Scope = "type",
    Target = "Silverlight.Media.ShoutcastMediaStreamSource",
    MessageId = "Mp",
    Justification = "Mp is not a two letter acyonym but is instead part of Mp3")]

namespace Silverlight.Media
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Windows;
    using System.Windows.Media;
    using Silverlight.Media.Metadata;

    /// <summary>
    /// A Simple MediaStreamSource which can play back MP3 streams from
    /// beginning to end.
    /// </summary>
    public class ShoutcastMediaStreamSource : MediaStreamSource, IDisposable
    {
        /// <summary>
        /// The current metadata for the Shoutcast stream.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "There is a public property representing the current metadata, which causes a naming conflict.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "This field is used in an Interlocked statement.")]
        internal ShoutcastMetadata currentMetadata = new ShoutcastMetadata();

        /// <summary>
        /// Exception set by the worker thread.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "This is an internal field only accessed by the private ShoutcastStream.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "This field is used in an Interlocked statement.")]
        internal Exception workerException;

        /// <summary>
        /// Empty dictionary of MediaSampleAttributeKeys, as they are unused in this MedaStreamSource.
        /// </summary>
        private static Dictionary<MediaSampleAttributeKeys, string> emptyDict = new Dictionary<MediaSampleAttributeKeys, string>();

        /// <summary>
        /// Current timestamp at which a sample should be rendered as measured in 100 nanosecond increments. 
        /// </summary>
        private long currentTimestamp;

        /// <summary>
        /// MediaStreamDescription for the associated Mp3 stream.
        /// </summary>
        private MediaStreamDescription audioStreamDescription;

        /// <summary>
        /// The Mp3 stream being played back.
        /// </summary>
        private ShoutcastStream audioStream;

        /// <summary>
        /// Initializes a new instance of the ShoutcastMediaStreamSource class.
        /// </summary>
        /// <param name="uri">Uri of the Mp3 stream.</param>
        public ShoutcastMediaStreamSource(Uri uri)
            : this(uri, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ShoutcastMediaStreamSource class.
        /// </summary>
        /// <param name="uri">Uri of the Mp3 stream.</param>
        /// <param name="includeMetadata">true to include metadata, otherwise, false.</param>
        public ShoutcastMediaStreamSource(Uri uri, bool includeMetadata)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            this.StreamUri = uri;
            this.IncludeMetadata = includeMetadata;
        }

        /// <summary>
        /// Finalizes an instance of the ShoutcastMediaStreamSource class.
        /// </summary>
        ~ShoutcastMediaStreamSource()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Fired when the Mp3 metadata changed.
        /// </summary>
        public event RoutedEventHandler MetadataChanged;

        /// <summary>
        /// Fired when the ShoutcastStream is closed.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Gets the Uri of the audio stream.
        /// </summary>
        public Uri StreamUri { get; private set; }

        /// <summary>
        /// Gets a value representing the current Shoutcast metadata.
        /// </summary>
        public ShoutcastMetadata CurrentMetadata
        {
            get { return this.currentMetadata; }
        }

        /// <summary>
        /// Gets a value indicating whether or not metadata is included in this MSS.
        /// </summary>
        internal bool IncludeMetadata { get; private set; }

        /// <summary>
        /// Releases all resources used by the MediaStreamSource.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "This mimics the System.IO.Stream Dispose() code")]
        public void Dispose()
        {
            // Like System.IO.Stream
            this.CloseMedia();
        }

        /// <summary>
        /// Fires the MetadataChanged event.
        /// </summary>
        internal void OnMetadataChanged()
        {
            RoutedEventHandler handler = this.MetadataChanged;
            if (handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Raises the Closed event.
        /// </summary>
        internal void OnClosed()
        {
            EventHandler handler = this.Closed;
            if (handler != null)
            {
                System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  OnClosed()");
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MediaStreamSource and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.CleanupAudioStream();
            }
        }

        /// <summary>
        /// Parses the passed in MediaStream to find the first frame and signals
        /// to its parent MediaElement that it is ready to begin playback by calling
        /// ReportOpenMediaCompleted.
        /// </summary>
        protected override void OpenMediaAsync()
        {
            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  OpenMediaAsync()");

            // So, here is why this is a little weird.
            // The Shoutcast server software has the ability to provide web pages.  These pages just happen to be served from the SAME address as the media stream.
            // Putting a "/;" at the end of the Uri will tell the Shoutcast server that we aren't a web browser, so stream the data.  The problem is that not ALL
            // Shoutcast servers are configured that way.  So, we have to do a request to get the content type.  If it is text/html, we append the "/;" and move on.
            // If it is an empty string, 99.9% of the time, this will be the media stream (If it's an ICY stream, the ICY "headers" don't parse properly).  The ShoutcastStream
            // will handle this case, so we let it go through.
            HttpWebRequest contentTypeRequest = ShoutcastMediaStreamSource.CreateHttpWebRequest(this.StreamUri, this.IncludeMetadata);
            contentTypeRequest.BeginGetResponse(
                ia1 =>
                {
                    HttpWebRequest req1 = ia1.AsyncState as HttpWebRequest;
                    try
                    {
                        HttpWebResponse res1 = (HttpWebResponse)req1.EndGetResponse(ia1);
                        string contentType = res1.ContentType;
                        if ((contentType == string.Empty) || (contentType == "audio/mpeg"))
                        {
                            try
                            {
                                this.audioStream = new ShoutcastStream(this, res1);
                                this.audioStreamDescription = this.audioStream.AudioStreamDescription;
                                this.ReportOpenMediaCompleted(this.audioStream.AudioSourceAttributes, new MediaStreamDescription[] { this.audioStream.AudioStreamDescription });
                            }
                            catch (Exception ex)
                            {
                                this.CleanupAudioStream();
                                this.ErrorOccurred(ex.Message);
                            }
                        }
                        else
                        {
                            // Close the original response.  We need another one.
                            res1.Close();
                            res1 = null;
                            if (!this.StreamUri.OriginalString.EndsWith("/", StringComparison.Ordinal))
                            {
                                this.StreamUri = new Uri(this.StreamUri.OriginalString + "/;", UriKind.Absolute);
                            }
                            else
                            {
                                this.StreamUri = new Uri(this.StreamUri.OriginalString + ";", UriKind.Absolute);
                            }

                            HttpWebRequest streamRequest = ShoutcastMediaStreamSource.CreateHttpWebRequest(this.StreamUri, this.IncludeMetadata);
                            streamRequest.BeginGetResponse(
                                ia =>
                                {
                                    HttpWebRequest req = ia.AsyncState as HttpWebRequest;
                                    try
                                    {
                                        HttpWebResponse res = (HttpWebResponse)req.EndGetResponse(ia);
                                        this.audioStream = new ShoutcastStream(this, res);
                                        this.audioStreamDescription = this.audioStream.AudioStreamDescription;
                                        this.ReportOpenMediaCompleted(this.audioStream.AudioSourceAttributes, new MediaStreamDescription[] { this.audioStream.AudioStreamDescription });
                                    }
                                    catch (Exception ex)
                                    {
                                        if (res1 != null)
                                        {
                                            res1.Close();
                                        }

                                        this.CleanupAudioStream();
                                        this.ErrorOccurred(ex.Message);
                                    }
                                },
                                streamRequest);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.CleanupAudioStream();
                        this.ErrorOccurred(ex.Message);
                    }
                },
                contentTypeRequest);
        }

        /// <summary>
        /// Parses the next sample from the requested stream and then calls ReportGetSampleCompleted
        /// to inform its parent MediaElement of the next sample.
        /// </summary>
        /// <param name="mediaStreamType">
        /// Should always be Audio for this MediaStreamSource.
        /// </param>
        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  GetSampleAsync()");

            // If the MSS has been disposed, but the player has not been stopped, this will force a stop by returning an empty sample.
            if (this.audioStream == null)
            {
                System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Race condition #1 handled!");
                this.ReportGetSampleCompleted(new MediaStreamSample(this.audioStreamDescription, null, 0, 0, 0, ShoutcastMediaStreamSource.emptyDict));
                return;
            }

            if (this.workerException != null)
            {
                System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Error #1 handled!");
                this.CleanupAudioStream();
                this.ErrorOccurred(this.workerException.Message);
                return;
            }

            // See if we need to report buffering. 
            int bufferingPercentage = this.audioStream.BufferingPercentage;
            while (bufferingPercentage < 100)
            {
                System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Buffering percentage less than 100");
                this.ReportGetSampleProgress(bufferingPercentage / 100.0d);

                // DANGER WILL ROBINSON!!! DANGER!!!
                // This line causes a race condition, as Thread.Sleep() causes the current thread to give up its time slice.  If the next thread scheduled to run is a thread that
                // is calling Dispose, our audio stream can be null, so we need to check after we wake up.  If so, we need to return an empty audio sample to shut everything down
                // properly.
                System.Threading.Thread.Sleep(10);

                // If the MSS has been disposed, but the player has not been stopped, this will force a stop by returning an empty sample.
                if (this.audioStream == null)
                {
                    System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Race condition #2 handled!");
                    this.ReportGetSampleCompleted(new MediaStreamSample(this.audioStreamDescription, null, 0, 0, 0, ShoutcastMediaStreamSource.emptyDict));
                    return;
                }

                if (this.workerException != null)
                {
                    System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Error #2 handled!");
                    this.ErrorOccurred(this.workerException.Message);
                    return;
                }

                bufferingPercentage = this.audioStream.BufferingPercentage;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("ReportGetSampleCompleted()");
                MediaStreamSample audioSample = new MediaStreamSample(
                        this.audioStreamDescription,
                        this.audioStream,
                        0,
                        this.audioStream.CurrentFrameSize,
                        this.currentTimestamp,
                        ShoutcastMediaStreamSource.emptyDict);

                this.currentTimestamp += this.audioStream.WaveFormat.AudioDurationFromBufferSize((uint)this.audioStream.CurrentFrameSize);
                this.ReportGetSampleCompleted(audioSample);
            }
            catch (Exception ex)
            {
                this.ErrorOccurred(ex.Message);
            }
        }

        /// <summary>
        ///  Closes down the open media streams and otherwise cleans up the MediaStreamSource. The MediaElement can call this method when going through normal shutdown or as a result of an error.
        /// </summary>
        protected override void CloseMedia()
        {
            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  CloseMedia()");
            System.Diagnostics.Debug.WriteLine("StackTrace:  {0}", new System.Diagnostics.StackTrace());

            // Call the dispose, the way System.IO.Stream works.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gathers the diagnostic information requested.
        /// </summary>
        /// <param name="diagnosticKind">
        /// A member of the MediaStreamSourceDiagnosticKind enumeration describing what type of information is desired.
        /// </param>
        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <para>
        /// Effectively a Null-Op for when a MediaElement requests a seek at the beginning
        /// of the stream. This makes the stream semi-unseekable.
        /// </para>
        /// <para>
        /// In a fuller MediaStreamSource, the logic here would be to actually seek to
        /// the correct mpeg frame matching the seekToTime passed in.
        /// </para>
        /// </summary>
        /// <param name="seekToTime">
        ///  The time to seek to in nanosecond ticks.
        /// </param>
        protected override void SeekAsync(long seekToTime)
        {
            this.ReportSeekCompleted(seekToTime);
        }

        /// <summary>
        /// Called when a stream switch is requested on the MediaElement.
        /// </summary>
        /// <param name="mediaStreamDescription">
        /// The stream switched to.
        /// </param>
        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an HttpWebRequest for streaming Shoutcast MP3 streams.
        /// </summary>
        /// <param name="uri">The Uri of the Shoutcast MP3 stream.</param>
        /// <param name="includeMetadata">Indicates whether or not to include metadata with the Shoutcast Mp3 stream.</param>
        /// <returns>An HttpWebRequest</returns>
        private static HttpWebRequest CreateHttpWebRequest(Uri uri, bool includeMetadata)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            if (includeMetadata)
            {
                request.Headers["icy-metadata"] = "1";
            }

            // We have to turn off ReadStreamBuffering, as it will try to download the whole stream before we can do anything, which is BAD!
            request.AllowReadStreamBuffering = false;

            return request;
        }

        /// <summary>
        /// Cleans up all associated streaming resources.
        /// </summary>
        private void CleanupAudioStream()
        {
            var tempStream = this.audioStream;
            this.audioStream = null;
            if (tempStream != null)
            {
                tempStream.Closed += (s, e) =>
                {
                    this.OnClosed();
                };

                tempStream.Dispose();
            }
        }
    }
}
