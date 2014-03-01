using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.BackgroundAudio;
using System.Collections.Generic;
using Windows.Foundation;
using System.IO.IsolatedStorage;

namespace AudioPlaybackAgent
{
    /// <summary>
    /// Shoutcast audio player for background audio.
    /// </summary>
    public class AudioPlayer : AudioPlayerAgent
    {
        public event EventHandler< PlayStateEventArgs> PlayStateChangedEA;


        /// <summary>
        /// 当前播放状态
        /// </summary>
        public static PlayState isoPlayState
        {
            get
            {
                return IsolatedStorageSettings.ApplicationSettings.Contains("isoPlayState") ? (PlayState)IsolatedStorageSettings.ApplicationSettings["isoPlayState"] : PlayState.Unknown;
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["isoPlayState"] = value;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        /// <summary>
        /// Static field that denotes if this class has been initialized.
        /// </summary>
        private static volatile bool classInitialized;

        /// <summary>
        /// Initializes a new instance of the AudioPlayer class.
        /// </summary>
        /// <remarks>
        /// AudioPlayer instances can share the same process. 
        /// Static fields can be used to share state between AudioPlayer instances
        /// or to communicate with the Audio Streaming agent.
        /// </remarks>
        public AudioPlayer()
        {
            if (!AudioPlayer.classInitialized)
            {
                AudioPlayer.classInitialized = true;

                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += this.AudioPlayer_UnhandledException;
                });
            }
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// <para>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback.
        /// </para>
        /// <para>
        /// Notable playstate events: 
        /// (a) TrackEnded: invoked when the player has no current track. The agent can set the next track.
        /// (b) TrackReady: an audio track has been set and it is now ready for playack.
        /// </para>
        /// <para>
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </para>
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  OnPlayStateChanged() - {0}", playState);
            if (PlayStateChangedEA!=null)
            {
                PlayStateChangedEA(this,new PlayStateEventArgs() { playState = playState });
            }

            isoPlayState = playState;

            switch (playState)
            {
                case PlayState.TrackEnded:
                    break;
                case PlayState.TrackReady:
                    player.Play();
                    break;
                case PlayState.Shutdown:
                    // TODO: Handle the shutdown state here (e.g. save state)
                    break;
                case PlayState.Unknown:
                    break;
                case PlayState.Stopped:
                    break;
                case PlayState.Paused:
                    break;
                case PlayState.Playing:
                    break;
                case PlayState.BufferingStarted:
                    break;
                case PlayState.BufferingStopped:
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.FastForwarding:
                    break;
            }

            NotifyComplete();
        }

        /// <summary>
        /// Called when the user requests an action using application/system provided UI
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported.
        /// Call NotifyComplete() only once, after the agent request has been completed, including async callbacks.
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  OnUserAction() - {0}", action);
            switch (action)
            {
                case UserAction.Play:
                    // Since we are just restarting the same stream, this should be fine.
                    player.Track = track;
                    break;
                case UserAction.Stop:
                case UserAction.Pause:
                    player.Stop();

                    // Stop the background streaming agent.
                    AudioTrackStreamer.ShutdownMediaStreamSource();//关闭
                    //Shoutcast.Sample.Phone.Background.Playback.AudioTrackStreamer.ShutdownMediaStreamSource();
                    break;
                case UserAction.FastForward:
                    break;
                case UserAction.Rewind:
                    break;
                case UserAction.Seek:
                    break;
                case UserAction.SkipNext:
                    break;
                case UserAction.SkipPrevious:
                    break;
            }

            NotifyComplete();
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            if (isFatal)
            {
                Abort();
            }
            else
            {
                player.Track = null;
                NotifyComplete();
            }
        }

        /// <summary>
        /// Called by the operating system to alert a background agent that it is going to be put into a dormant state or terminated.
        /// </summary>
        protected override void OnCancel()
        {
            base.OnCancel();
            this.NotifyComplete();
        }

        /// <summary>
        /// Code to execute unhandled exceptions.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">ApplicationUnhandledExceptionEventArgs associated with this event.</param>
        private void AudioPlayer_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }
    }

    #region 后台播放代码
    //public class AudioPlayer : AudioPlayerAgent
    //{
    //    private static volatile bool _classInitialized;
    //    /// <summary>
    //    ///当前播放
    //    /// </summary>
    //    private static int _currentTack = 0;

    //    private  static List<AudioTrack> fmPlayList= new List<Microsoft.Phone.BackgroundAudio.AudioTrack>()
    //        {
    //             new AudioTrack(new Uri("mms://a1450.l11459845449.c114598.g.lm.akamaistream.net/D/1450/114598/v0001/reflector:45449", UriKind.Absolute),"Y.E.S 933FM","KissRadio","",null),
    //              new AudioTrack(new Uri("mms://a1450.l11459845449.c114598.g.lm.akamaistream.net/D/1450/114598/v0001/reflector:45449", UriKind.Absolute),"Y.E.S 933FM","KissRadio","",null),
    //        };


    //    /// <remarks>
    //    /// AudioPlayer 实例可共享同一进程。
    //    /// 静态字段可用于 AudioPlayer 实例之间共享状态
    //    /// 或与音频流代理通信。
    //    /// </remarks>
    //    static AudioPlayer()
    //    {
    //        // 订阅托管异常处理程序
    //        Deployment.Current.Dispatcher.BeginInvoke(delegate
    //        {
    //            Application.Current.UnhandledException += UnhandledException;
    //        });
    //    }

    //    /// 出现未处理的异常时执行的代码
    //    private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
    //    {
    //        if (Debugger.IsAttached)
    //        {
    //            // 出现未处理的异常；强行进入调试器
    //            Debugger.Break();
    //        }
    //    }

    //    /// <summary>
    //    /// playstate 更改时调用，但 Error 状态除外(参见 OnError)
    //    /// </summary>
    //    /// <param name="player">BackgroundAudioPlayer</param>
    //    /// <param name="track">在 playstate 更改时播放的曲目</param>
    //    /// <param name="playState">播放机的新 playstate </param>
    //    /// <remarks>
    //    /// 无法取消播放状态更改。即使应用程序
    //    /// 导致状态自行更改也会提出这些更改，假定应用程序已经选择了回调。
    //    ///
    //    /// 值得注意的 playstate 事件: 
    //    /// (a) TrackEnded:  播放器没有当前曲目时激活。代理可设置下一曲目。
    //    /// (b) TrackReady:  音轨已设置完毕，现在可以播放。
    //    ///
    //    /// 只在代理请求完成之后调用一次 NotifyComplete()，包括异步回调。
    //    /// </remarks>
    //    protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
    //    {
    //        switch (playState)
    //        {
    //            case PlayState.TrackEnded:
    //                player.Track = GetPreviousTrack();
    //                break;
    //            case PlayState.TrackReady:
    //                player.Play();
    //                break;
    //            case PlayState.Shutdown:
    //                // TODO: 在此处理关机状态(例如保存状态)
    //                break;
    //            case PlayState.Unknown:
    //                break;
    //            case PlayState.Stopped:
    //                break;
    //            case PlayState.Paused:
    //                break;
    //            case PlayState.Playing:
    //                break;
    //            case PlayState.BufferingStarted:
    //                break;
    //            case PlayState.BufferingStopped:
    //                break;
    //            case PlayState.Rewinding:
    //                break;
    //            case PlayState.FastForwarding:
    //                break;
    //        }

    //        NotifyComplete();
    //    }

    //    /// <summary>
    //    /// 在用户使用应用程序/系统提供的用户界面请求操作时调用
    //    /// </summary>
    //    /// <param name="player">BackgroundAudioPlayer</param>
    //    /// <param name="track">用户操作期间播放的曲目</param>
    //    /// <param name="action">用户请求的操作</param>
    //    /// <param name="param">与请求的操作相关联的数据。
    //    /// 在当前版本中，此参数仅适合与 Seek 操作一起使用，
    //    /// 以指明请求的乐曲的位置</param>
    //    /// <remarks>
    //    /// 用户操作不自动对系统状态进行任何更改；如果用户操作受支持，
    //    /// 以便执行用户操作(如果这些操作受支持)。
    //    ///
    //    /// 只在代理请求完成之后调用 NotifyComplete() 一次，包括异步回调。
    //    /// </remarks>
    //    protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
    //    {
    //        switch (action)
    //        {
    //            case UserAction.Play:
    //                if (player.PlayerState != PlayState.Playing)
    //                {
    //                    player.Track = fmPlayList[_currentTack];
    //                    Debug.WriteLine( player.BufferingProgress);
    //                    player.Play();
    //                }
    //                break;
    //            case UserAction.Stop:
    //                player.Stop();
    //                break;
    //            case UserAction.Pause:
    //                player.Pause();
    //                break;
    //            case UserAction.FastForward:
    //                player.FastForward();
    //                break;
    //            case UserAction.Rewind:
    //                player.Rewind();
    //                break;
    //            case UserAction.Seek:
    //                player.Position = (TimeSpan)param;
    //                break;
    //            case UserAction.SkipNext:
    //                player.Track = GetNextTrack();
    //                break;
    //            case UserAction.SkipPrevious:
    //                AudioTrack previousTrack = GetPreviousTrack();
    //                if (previousTrack != null)
    //                {
    //                    player.Track = previousTrack;
    //                }
    //                break;
    //        }

    //        NotifyComplete();
    //    }

    //    /// <summary>
    //    /// 实现逻辑以获取下一个 AudioTrack 实例。
    //    /// 在播放列表中，源可以是文件、Web 请求，等等。
    //    /// </summary>
    //    /// <remarks>
    //    /// AudioTrack URI 确定源，源可以是: 
    //    /// (a) 独立存储器文件(相对 URI，表示独立存储器中的路径)
    //    /// (b) HTTP URL(绝对 URI)
    //    /// (c) MediaStreamSource (null)
    //    /// </remarks>
    //    /// <returns>AudioTrack 实例，或如果播放完毕，则返回 null</returns>
    //    private AudioTrack GetNextTrack()
    //    {
    //        // TODO:  添加逻辑以获取下一条音轨
    //        if (++_currentTack>=fmPlayList.Count)
    //        {
    //            _currentTack = 0;
    //        }
    //        AudioTrack track = fmPlayList[_currentTack];

    //        // 指定曲目

    //        return track;
    //    }

    //    /// <summary>
    //    /// 实现逻辑以获取前一个 AudioTrack 实例。
    //    /// </summary>
    //    /// <remarks>
    //    /// AudioTrack URI 确定源，它可以是: 
    //    /// (a) 独立存储器文件(相对 URI，表示独立存储器中的路径)
    //    /// (b) HTTP URL(绝对 URI)
    //    /// (c) MediaStreamSource (null)
    //    /// </remarks>
    //    /// <returns>AudioTrack 实例，或如果不允许前一曲目，则返回 null</returns>
    //    private AudioTrack GetPreviousTrack()
    //    {
    //        // TODO:  添加逻辑以获取前一条音轨
    //        if (--_currentTack<0)
    //        {
    //            _currentTack = fmPlayList.Count - 1;
    //        }
    //        AudioTrack track = fmPlayList[_currentTack];

    //        // 指定曲目

    //        return track;
    //    }

    //    /// <summary>
    //    /// 每次播放出错(如 AudioTrack 未正确下载)时调用
    //    /// </summary>
    //    /// <param name="player">BackgroundAudioPlayer</param>
    //    /// <param name="track">出现错误的曲目</param>
    //    /// <param name="error">出现的错误</param>
    //    /// <param name="isFatal">如果为 true，则播放不能继续并且曲目播放将停止</param>
    //    /// <remarks>
    //    /// 不保证在所有情况下都调用此方法。例如，如果后台代理程序
    //    /// 本身具有未处理的异常，则不会回调它来处理它自己的错误。
    //    /// </remarks>
    //    protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
    //    {
    //        if (isFatal)
    //        {
    //            Abort();
    //        }
    //        else
    //        {
    //            NotifyComplete();
    //        }

    //    }

    //    /// <summary>
    //    /// 取消代理请求时调用
    //    /// </summary>
    //    /// <remarks>
    //    /// 取消请求后，代理需要 5 秒钟完成其工作，
    //    /// 通过调用 NotifyComplete()/Abort()。
    //    /// </remarks>
    //    protected override void OnCancel()
    //    {

    //    }
    //} 
    #endregion
}