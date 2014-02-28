using System;
using System.Windows;
using Microsoft.Phone.BackgroundAudio;
using Silverlight.Media;

namespace AudioPlaybackAgent
{
    /// <summary>
    /// A background agent that performs per-track streaming for playback
    /// </summary>
    public class AudioTrackStreamer : AudioStreamingAgent
    {
        /// <summary>
        /// Static field that contains the ShoutcastMediaStreamSource associated with this AudioStreamingAgent.
        /// </summary>
        private static ShoutcastMediaStreamSource mss;

        /// <summary>
        /// Static field used to synchronize access to the ShoutcastMediaStreamSource associated with this AudioStreamingAgent.
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the AudioTrackStreamer class.
        /// </summary>
        public AudioTrackStreamer()
            : base()
        {
        }

        /// <summary>
        /// Completely shuts down the AudioTrackStreamer.
        /// </summary>
        public static void ShutdownMediaStreamSource()
        {
            if (AudioTrackStreamer.mss != null)
            {
                lock (AudioTrackStreamer.syncRoot)
                {
                    if (AudioTrackStreamer.mss != null)
                    {
                        // Because of the NotifyComplete(), we need to set this BEFORE the MSS ends.
                        ShoutcastMediaStreamSource temp = AudioTrackStreamer.mss;
                        AudioTrackStreamer.mss = null;
                        temp.MetadataChanged -= new System.Windows.RoutedEventHandler(AudioTrackStreamer.MetadataChanged);
                        temp.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Called when a new track requires audio decoding
        /// (typically because it is about to start playing)
        /// </summary>
        /// <param name="track">
        /// The track that needs audio streaming
        /// </param>
        /// <param name="streamer">
        /// The AudioStreamer object to which a MediaStreamSource should be
        /// attached to commence playback
        /// </param>
        /// <remarks>
        /// To invoke this method for a track set the Source parameter of the AudioTrack to null
        /// before setting  into the Track property of the BackgroundAudioPlayer instance
        /// property set to true;
        /// otherwise it is assumed that the system will perform all streaming
        /// and decoding
        /// </remarks>
        protected override void OnBeginStreaming(AudioTrack track, AudioStreamer streamer)
        {
            lock (AudioTrackStreamer.syncRoot)
            {
                AudioTrackStreamer.mss = new ShoutcastMediaStreamSource(new Uri(track.Tag));
                AudioTrackStreamer.mss.MetadataChanged += new RoutedEventHandler(AudioTrackStreamer.MetadataChanged);
                AudioTrackStreamer.mss.Closed += (s, e) =>
                {
                    this.NotifyComplete();
                };
                streamer.SetSource(AudioTrackStreamer.mss);
            }
        }

        /// <summary>
        /// Called when the agent request is getting cancelled
        /// The call to base.OnCancel() is necessary to release the background streaming resources
        /// </summary>
        protected override void OnCancel()
        {
            base.OnCancel();

            // The shutdown calls NotifyComplete(), so we don't have to call it here.
            AudioTrackStreamer.ShutdownMediaStreamSource();
        }

        /// <summary>
        /// Code to execute when the Shoutcast stream metadata changes.
        /// </summary>
        /// <param name="sender">Send of the event.</param>
        /// <param name="e">RoutedEventArgs associated with this event.</param>
        private static void MetadataChanged(object sender, RoutedEventArgs e)
        {
            var track = BackgroundAudioPlayer.Instance.Track;
            if (track != null)
            {
                track.BeginEdit();
                track.Artist = AudioTrackStreamer.mss.CurrentMetadata.Title;
                track.EndEdit();
            }
        }
    }
}
