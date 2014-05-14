/*
 *代码很乱很糟糕，fuck。
 *
 */

using AudioPlaybackAgent;
using FMRadioPro.Data;
using FMRadioPro.Utilities;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using UmengSDK;
using UmengSocialSDK;
using UmengSocialSDK.Net.Request;

namespace FMRadioPro
{
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        ///更新UI计时器
        /// </summary>
        private DispatcherTimer timerr;

        /// <summary>
        /// 播放列表
        /// </summary>
        private List<AudioTrack> _playList;

        /// <summary>
        /// 当前播放
        /// </summary>
        public static int gCurrentTrack = 0;

        private AudioPlayer audioPlayer = new AudioPlayer();

        private bool isplay = false;

        // AudioCategory
        // 构造函数
        public MainPage()
        {
            InitializeComponent();
            // InitPathMenu();

            BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
            audioPlayer.PlayStateChangedEA += audioPlayer_PlayStateChangedEA;
            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();

            this.Loaded += MainPage_Loaded;
            this.btnBack.Click += btnBack_Click;
            this.btnPlay.Click += btnPlay_Click;
            this.btnPause.Click += btnPause_Click;
            this.btnNext.Click += btnNext_Click;
            //this.btnStop.Click += btnStop_Click;
            this.listRadioList.SelectionChanged += listRadioList_SelectionChanged;
            //btnOption.Click += btnOption_Click;
            btnShare.Click += btnShare_Click;
        }

        /// <summary>
        /// 分享按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnShare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                WriteableBitmap bitmap = await Screen();

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.SaveJpeg(memoryStream, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
                    bitmapImage.SetSource(memoryStream);
                }

                ShareData shareData = new ShareData();
                string content = "";
                if (string.IsNullOrWhiteSpace(txtPlayName.Text))
                {
                    content = "我正在使用FMRadioPro收听广播，分享一个好APP，支持CodeMonkey @十一_x 写APP赚钱娶媳妇。 ";
                }
                else
                {
                    content = string.Format("我正在使用FMRadioPro收听{0}，分享一个好APP，支持CodeMonkey @十一_x 写APP赚钱娶媳妇。", txtPlayName.Text);
                }
                shareData.Content = content;

                shareData.Picture = bitmapImage;

                ShareOption option = new ShareOption();
                option.ShareCompleted = args =>
                {
                    if (args.StatusCode == UmengSocialSDK.UmEventArgs.Status.Successed)
                    {
                        //分享成功
                        // MessageBox.Show("分享成功");
                    }
                    else
                    {
                        //分享失败
                        //MessageBox.Show("分享失败");
                    }
                };

                UmengSocial.Share(AppConfig.AppKey, shareData, null, this, option);
            }
            catch (Exception ex)
            {
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        private async Task<WriteableBitmap> Screen()
        {
            //截图
            await Task.Delay(200);

            using (System.IO.Stream stream1 = Application.GetResourceStream(new Uri(@"Assets/qcode.png", UriKind.Relative)).Stream)
            {
                double width = Application.Current.Host.Content.ActualWidth;
                double heigth = Application.Current.Host.Content.ActualHeight;
                WriteableBitmap wbmp = new WriteableBitmap((int)width, (int)heigth);
                wbmp.Render(App.Current.RootVisual, null);
                wbmp.Invalidate();


                WriteableBitmap wideBitmap = new WriteableBitmap((int)width, (int)(heigth + 280));

                BitmapImage image1 = new BitmapImage();
                image1.SetSource(stream1);
                stream1.Close();
                //stream1.Flush();

                wideBitmap.Blit(new Rect(0, 0, width, heigth), new WriteableBitmap(wbmp), new Rect(0, 0, wbmp.PixelWidth, wbmp.PixelHeight), WriteableBitmapExtensions.BlendMode.Additive);
                wideBitmap.Blit(new Rect((width - 280) / 2, heigth, 280, 280), new WriteableBitmap(image1), new Rect(0, 0, image1.PixelWidth, image1.PixelHeight), WriteableBitmapExtensions.BlendMode.Additive);

                return wideBitmap;
            }
        }

        /// <summary>
        /// 关于
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            //TODO:Aobut.xaml
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void audioPlayer_PlayStateChangedEA(object sender, PlayStateEventArgs e)
        {
            //txtPlayState.Text =""+ e.playState;
            Debug.WriteLine("-----------------------" + e.playState);
        }

        private void UpdateButton()
        {
        }

        private void UpdateState(object sender, EventArgs e)
        {
            try
            {
                AudioTrack audioTrack = BackgroundAudioPlayer.Instance.Track;

                if (audioTrack != null)
                {
                    txtPlayName.Text = audioTrack.Title;
                    if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
                    {
                        btnPause.Visibility = Visibility.Visible;
                        btnPlay.Visibility = Visibility.Collapsed;
                        //TODO:显示当前播放内容
                    }
                    else
                    {
                        btnPause.Visibility = Visibility.Collapsed;
                        btnPlay.Visibility = Visibility.Visible;
                        //TODO:播放内容清空
                    }
                }
            }
            catch (Exception)
            {
                timerr.Stop();
            }
        }

        private void listRadioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectenItem = (RadiosInfo)listRadioList.SelectedItem;
                listRadioList.ScrollTo(selectenItem);

                AudioTrack selectAudioTrack = new AudioTrack(new Uri(selectenItem.URL, UriKind.Absolute), selectenItem.Name, selectenItem.NamePinyin, "", null, "", EnabledPlayerControls.Pause);
                //TODO：查找当前Select项在_PlayList中的index，然后给 isoCurrentTrack

                foreach (var item in _playList)
                {
                    if (item.Source == selectAudioTrack.Source)
                    {
                        selectAudioTrack = item;
                    }
                }

                int index = _playList.IndexOf(selectAudioTrack, 0);

                AppConfig.isoCurrentTrack = index;

                ////TODO:播放当前选择项
                //Debug.WriteLine("SelectItem Radio URL:" + selectenItem.URL);
                //Debug.WriteLine("SelectenItem Radio Index:" + index);
                //BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];

                //if (PlayState.Unknown == BackgroundAudioPlayer.Instance.PlayerState)
                //{
                //    isplay = true;
                //    //BackgroundAudioPlayer.Instance.Stop();
                //    BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];
                //    BackgroundAudioPlayer.Instance.Play();
                //}

                //else
                //{
                //    //BackgroundAudioPlayer.Instance.Stop();
                //    BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];
                //    // BackgroundAudioPlayer.Instance.Play();
                //    //this.UpdateState(null, null);
                //}
                //    this.UpdateState(null, null);

                //BackgroundAudioPlayer.Instance.Volume = 1.0d;

                try
                {
                    ////BackgroundAudioPlayer.Instance.Track = null;// new AudioTrack();
                    BackgroundAudioPlayer.Instance.Track = _playList[index];
                    BackgroundAudioPlayer.Instance.Volume = 1.0d;
                    ////BackgroundAudioPlayer.Instance.Play();

                    if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
                    {
                        BackgroundAudioPlayer.Instance.Pause();
                    }
                    else if (PlayState.Unknown == BackgroundAudioPlayer.Instance.PlayerState)
                    {
                        //   BackgroundAudioPlayer.Instance.Stop();

                        BackgroundAudioPlayer.Instance.Track = _playList[index];
                        BackgroundAudioPlayer.Instance.Play();
                    }
                    else
                    {
                        BackgroundAudioPlayer.Instance.Track = _playList[index];
                    }

                    Debug.WriteLine("Play_Click Play:" + index);
                }
                catch (Exception ex)
                {
                    UmengSDK.UmengAnalytics.TrackException(ex);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("listRadioList_SelectionChanged" + ex.ToString());
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        /// <summary>
        /// 播放状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            try
            {
                BackgroundAudioPlayer play = sender as BackgroundAudioPlayer;
                if (play.Error != null)
                {
                    //TODO:处理后台播放错误
                }

                PlayState playState;

                try
                {
                    playState = BackgroundAudioPlayer.Instance.PlayerState;
                }
                catch (InvalidOperationException ex)
                {
                    playState = PlayState.Stopped;
                    UmengSDK.UmengAnalytics.TrackException(ex);
                }

                switch (playState)
                {
                    case PlayState.Unknown:
                        //BackgroundAudioPlayer.Instance.Stop();
                        //BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];
                        //BackgroundAudioPlayer.Instance.Play();
                        //BackgroundAudioPlayer.Instance.Stop();
                        break;

                    case PlayState.Paused:
                        this.UpdateState(null, null);
                        this.timerr.Stop();
                        break;

                    case PlayState.Playing:

                        this.UpdateState(null, null);

                        this.timerr.Start();
                        break;

                    case PlayState.Stopped:
                        this.timerr.Stop();
                        this.UpdateState(null, null);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PlayStateChanged:" + ex.ToString());
                UmengSDK.UmengAnalytics.TrackException(ex);
            }

            #region MyRegion

            //PlayStateChangedEventArgs newEventArgs = (PlayStateChangedEventArgs)e;

            //switch (BackgroundAudioPlayer.Instance.PlayerState)
            //{
            //    case PlayState.BufferingStarted:
            //        //TODO:缓存进度条
            //        Debug.WriteLine("11ing。。。。");
            //        break;
            //    case PlayState.BufferingStopped:
            //        //TODO:停止缓充
            //        Debug.WriteLine("end。。。。。。。");
            //        break;
            //    case PlayState.Error:
            //        break;
            //    case PlayState.FastForwarding:
            //        break;

            //    case PlayState.Playing:
            //        btnPause.Visibility = Visibility.Visible;
            //        btnPlay.Visibility = Visibility.Collapsed;
            //        break;
            //    case PlayState.Rewinding:
            //        break;
            //    case PlayState.Shutdown:
            //        //TODO:应用退出提示是否继续后台播放，否，停止播放
            //        break;
            //    case PlayState.Paused:
            //    case PlayState.Stopped:
            //        btnPause.Visibility = Visibility.Collapsed;
            //        btnPlay.Visibility = Visibility.Visible;
            //        break;
            //    case PlayState.TrackEnded:
            //        break;
            //    case PlayState.TrackReady:
            //        break;
            //    case PlayState.Unknown:
            //        break;
            //    default:
            //        break;

            //}
            //if (BackgroundAudioPlayer.Instance.Track!=null)
            //{
            //    //TODO:显示当前播放内容
            //}

            #endregion MyRegion
        }

        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMessageBox cuMessageBox = new CustomMessageBox()
                {
                    Caption = "停止播放并退出？",
                    Message = "",
                    LeftButtonContent = "NO",
                    RightButtonContent = "YES",
                };

                cuMessageBox.Dismissed += (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            // Do something.

                            break;

                        case CustomMessageBoxResult.RightButton:
                            // Do something.

                            this.timerr.Stop();
                            BackgroundAudioPlayer.Instance.Stop();
                            this.UpdateState(null, null);
                            Debug.WriteLine("Stop_Click Play:" + AppConfig.isoCurrentTrack);

                            FMRadioTool.StopRadioPlay();//停止播放

                            Application.Current.Terminate();//退出应用程序
                            break;

                        case CustomMessageBoxResult.None:
                            // Do something.
                            break;

                        default:
                            break;
                    }
                };

                cuMessageBox.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("btnStop_Click" + ex.ToString());
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        /// <summary>
        /// 下一曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //BackgroundAudioPlayer.Instance.SkipNext();
                if (++AppConfig.isoCurrentTrack >= _playList.Count)
                {
                    AppConfig.isoCurrentTrack = 0;
                }
                BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];
                //BackgroundAudioPlayer.Instance.Play();
                this.UpdateState(null, null);

                Debug.WriteLine("Next_Click Play:" + AppConfig.isoCurrentTrack);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("btnNext_Click" + ex.ToString());
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!BackgroundAudioPlayer.Instance.CanPause)
                //{
                //    return;
                //}
                BackgroundAudioPlayer.Instance.Pause();
                Debug.WriteLine("Pause_Click Play:" + AppConfig.isoCurrentTrack);
            }
            catch (Exception ex)
            {
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ////BackgroundAudioPlayer.Instance.Track = null;// new AudioTrack();
                BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];
                BackgroundAudioPlayer.Instance.Volume = 1.0d;
                ////BackgroundAudioPlayer.Instance.Play();

                if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
                {
                    BackgroundAudioPlayer.Instance.Pause();
                }
                else if (PlayState.Unknown == BackgroundAudioPlayer.Instance.PlayerState)
                {
                    //   BackgroundAudioPlayer.Instance.Stop();

                    BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];
                    BackgroundAudioPlayer.Instance.Play();
                }
                else
                {
                    BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];
                }

                Debug.WriteLine("Play_Click Play:" + AppConfig.isoCurrentTrack);
            }
            catch (Exception ex)
            {
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        /// <summary>
        /// 上一曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // BackgroundAudioPlayer.Instance.SkipPrevious();
                if (--AppConfig.isoCurrentTrack < 0)
                {
                    AppConfig.isoCurrentTrack = _playList.Count - 1;
                }
                BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];

                //listRadioList.ScrollTo(selectenItem);
                this.UpdateState(null, null);
                Debug.WriteLine("Back_Click Play:" + AppConfig.isoCurrentTrack);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("btnBack_Click" + ex.ToString());
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.timerr = new DispatcherTimer();
                this.timerr.Interval = TimeSpan.FromSeconds(.05);
                this.timerr.Tick += UpdateState;
            }
            catch (Exception ex)
            {
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                Debug.WriteLine(AppConfig.isoCurrentTrack);
                gCurrentTrack = AppConfig.isoCurrentTrack;

                if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
                {
                    btnPause.Visibility = Visibility.Visible;
                    btnPlay.Visibility = Visibility.Collapsed;
                    //TODO:显示当前播放内容
                }

                else
                {
                    btnPause.Visibility = Visibility.Collapsed;
                    btnPlay.Visibility = Visibility.Visible;
                    //TODO:播放内容清空
                }

                this.UpdateState(null, null);

                //TODO：移到页面加载时加载
                await Task.Run(() =>
                    {
                        this.listRadioList.Dispatcher.BeginInvoke(() =>
                            {
                                if (RadiosData.GetRadioData().Count <= 30)
                                {
                                    listRadioList.IsGroupingEnabled = false;
                                    listRadioList.ItemsSource = RadiosData.GetRadioData();
                                }
                                else
                                {
                                    listRadioList.IsGroupingEnabled = true;
                                    listRadioList.ItemsSource = RadiosData.GetData();
                                }

                                _playList = new List<AudioTrack>();
                                if (listRadioList.ItemsSource != null)
                                {
                                    List<RadiosInfo> radios = RadiosData.GetRadioData();

                                    foreach (var item in radios)
                                    {
                                        _playList.Add(new AudioTrack(new Uri(item.URL, UriKind.Absolute), item.Name, item.NamePinyin, "", null, "", EnabledPlayerControls.Pause));
                                    }
                                    //AppConfig.isoPlayTrack = _playList;
                                }
                            });
                    });
                base.OnNavigatedTo(e);
                UmengAnalytics.TrackPageStart("MainPage");
            }
            catch (Exception ex)
            {
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UmengAnalytics.TrackPageEnd("MainPage");
        }

        private void btnLocaFM_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnInterFM_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).DataContext;
            listRadioList.ItemsSource.Remove(item);
            //listRadioList.r
        }

        private void btnOption_Click_1(object sender, System.EventArgs e)
        {
            // 在此处添加事件处理程序实现。
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void btnStop_Click_1(object sender, System.EventArgs e)
        {
            //TODO：该功能移到开始页面
            // 在此处添加事件处理程序实现。
            try
            {
                CustomMessageBox cuMessageBox = new CustomMessageBox()
                {
                    Caption = "停止播放并退出？",
                    Message = "",
                    LeftButtonContent = "NO",
                    RightButtonContent = "YES",
                };

                cuMessageBox.Dismissed += (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            // Do something.

                            break;

                        case CustomMessageBoxResult.RightButton:
                            // Do something.

                            this.timerr.Stop();
                            BackgroundAudioPlayer.Instance.Stop();
                            this.UpdateState(null, null);
                            Debug.WriteLine("Stop_Click Play:" + AppConfig.isoCurrentTrack);

                            FrameworkDispatcher.Update();
                            MediaPlayer.Stop();
                            FrameworkDispatcher.Update();
                            MediaPlayer.Play(Song.FromUri("Snooze It!", new Uri("Audio/Void.wav", UriKind.Relative)));
                            FrameworkDispatcher.Update();

                            MediaPlayer.Stop();
                            FrameworkDispatcher.Update();

                            Application.Current.Terminate();//退出应用程序
                            break;

                        case CustomMessageBoxResult.None:
                            // Do something.
                            break;

                        default:
                            break;
                    }
                };

                cuMessageBox.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("btnStop_Click" + ex.ToString());
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        // 用于生成本地化 ApplicationBar 的示例代码
        //private void BuildLocalizedApplicationBar()
        //{
        //    // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
        //    ApplicationBar = new ApplicationBar();

        //    // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // 使用 AppResources 中的本地化字符串创建新菜单项。
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}