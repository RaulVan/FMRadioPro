using AudioPlaybackAgent;
using FMRadioPro.Data;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using UmengSDK;
using UmengSocialSDK;
using UmengSocialSDK.Net.Request;
using System.IO;

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
            //List<string> data=new List<string> ();
            //for (int i = 0; i < 100; i++)
            //{
            //    data.Add(i + "/Deanna 频道 test频道 test频道 test频道 test");
            //}
            //listRadioList.ItemsSource = data;
            // gridPanel.Width = Application.Current.Host.Content.ActualWidth * 2;
            this.Loaded += MainPage_Loaded;
            this.btnBack.Click += btnBack_Click;
            this.btnPlay.Click += btnPlay_Click;
            this.btnPause.Click += btnPause_Click;
            this.btnNext.Click += btnNext_Click;
            //this.btnStop.Click += btnStop_Click;
            this.listRadioList.SelectionChanged += listRadioList_SelectionChanged;
            btnOption.Click += btnOption_Click;
            btnShare.Click += btnShare_Click;
            //this.topMenBar.ManipulationDelta += topMenBar_ManipulationDelta;
            //this.topMenBar.ManipulationCompleted += topMenBar_ManipulationCompleted;

            //topMenBar.Visibility = Visibility.Collapsed;

            //Application.Current.UnhandledException += (a, b) =>
            //    {
            //        Debug.WriteLine("Exit--------------------------------");
            //    };
        }

        /// <summary>
        /// 分享按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnShare_Click(object sender, RoutedEventArgs e)
        {
            //SharePlatform platform = SharePlatform.Sina;
            //if (!UmengSocial.CheckAuthorized(platform))
            //{
            //    //MessageBox.Show("该平台未授权，请先进行授权！");
            //    //return;

            //    UmengSocial.Authorize(AppConfig.AppKey, platform, this, args =>
            //        {
            //            if (args.StatusCode==UmengSocialSDK.UmEventArgs.Status.Successed)
            //            {
            //                BitmapImage bitmapImage = new BitmapImage();
            //                bitmapImage.UriSource = new Uri("/Assets/qcode.png", UriKind.Relative);


            //                ShareData shareData = new ShareData();

            //                shareData.Content = "分享一个好APP，支持CodeMonkey 写代码赚钱娶媳妇。WP商场： http://www.windowsphone.com/s?appid=3e6b465b-e8fc-4c06-a64a-b4bec05e60cf ";
            //                //shareData.Url.Link = @"http://www.windowsphone.com/s?appid=3e6b465b-e8fc-4c06-a64a-b4bec05e60cf";
            //                //shareData.Url.Type = UrlType.Picture;
            //                //shareData.Url.Author = "FMRadioPro";
            //                //shareData.Url.Title = "情书";

            //                // WriteableBitmap ShareImage =
            //                shareData.Picture = bitmapImage;

            //                ShareOption option = new ShareOption();
            //                option.ShareCompleted = argss =>
            //                {
            //                    if (argss.StatusCode == UmengSocialSDK.UmEventArgs.Status.Successed)
            //                    {
            //                        //分享成功
            //                       // MessageBox.Show("分享成功");
            //                    }
            //                    else
            //                    {
            //                        //分享失败          
            //                        MessageBox.Show("分享失败");
            //                    }
            //                };

            //                UmengSocial.Share(AppConfig.AppKey, shareData, null, this, option);
            //            }
            //        });
            //}
            //else
            //{

            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri("/Assets/qcode.png", UriKind.Relative);


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
                //shareData.Url.Link = @"http://www.windowsphone.com/s?appid=3e6b465b-e8fc-4c06-a64a-b4bec05e60cf";
                //shareData.Url.Type = UrlType.Picture;
                //shareData.Url.Author = "FMRadioPro";
                //shareData.Url.Title = "情书";


                // WriteableBitmap ShareImage =
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
                        MessageBox.Show("分享失败");
                    }
                };

                UmengSocial.Share(AppConfig.AppKey, shareData, null, this, option);
            }
            catch (Exception ex)
            {
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }





        //}


        private void btnOption_Click(object sender, RoutedEventArgs e)
        {
            //TODO:Aobut.xaml
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        //private void InitPathMenu()
        //{
        //    double width = Application.Current.Host.Content.ActualWidth;
        //    double height = Application.Current.Host.Content.ActualHeight;
        //    Rect rc = new Rect { Width = 280, Height = 600 };
        //    var items = new List<AwesomMenuItem>();

        //    items.Add(new AwesomMenuItem("Images/icon-star.png", "Images/bg-menuitem.png"));
        //    items.Add(new AwesomMenuItem("Images/icon-star.png", "Images/bg-menuitem.png"));
        //    items.Add(new AwesomMenuItem("Images/icon-star.png", "Images/bg-menuitem.png"));
        //    items.Add(new AwesomMenuItem("Images/icon-star.png", "Images/bg-menuitem.png"));
        //    //items.Add(new AwesomMenuItem("Images/icon-star.png", "Images/bg-menuitem.png"));
        //    //items.Add(new AwesomMenuItem("Images/icon-star.png", "Images/bg-menuitem.png"));
        //    //items.Add(new AwesomMenuItem("Images/icon-star.png", "Images/bg-menuitem.png"));

        //    //构造的时候可以设置指定方法也可以通过方法来设置，都可以
        //    //var menu = new AwesomeMenu(rc, items, "Images/icon-plus.png", "Images/bg-addbutton.png", AwesomeMenuType.AwesomeMenuTypeDownAndRight);
        //    var menu = new AwesomeMenu(rc, items, "Images/icon-plus.png", "Images/bg-addbutton.png", new System.Windows.Point(width-50, height-50));
        //    //menu.Background = new SolidColorBrush(Colors.Cyan);
        //    //menu.SetType(AwesomeMenuType.AwesomeMenuTypeUpAndLeft);
        //    //menu.SetStartPoint(new Point(0, 150));

        //    menu.TapToDissmissItem = true;
        //    menu.AwesomeMenuRadianType = AwesomeMenuRadianType.AwesomeMenuRadianNone;
        //    menu.MenuItemSpacing = 0;
        //    this.gridPanel.Children.Add(menu);
        //    //ContentPanel.Children.Add(menu);
        //    menu.ActionClosed += (item) =>
        //    {
        //        Dispatcher.BeginInvoke(delegate
        //        {
        //            ProcessItem(item);
        //        });
        //    };
        //}

        //private void ProcessItem(AwesomMenuItem item)
        //{
        //    if (item != null)
        //    {
        //        if (item != null && !item.Tag.Equals(999))
        //        {
        //            int index = Convert.ToInt32(item.Tag);
        //            MessageBox.Show(string.Format("Item {0} Clicked!", index));
        //            //switch (index)
        //            //{
        //            //    case 0:
        //            //        MessageBox.Show(string.Format("Item {0} Clicked!", index));
        //            //        break;
        //            //    case 1:
        //            //        MessageBox.Show(string.Format("Item {0} Clicked!", index));
        //            //        break;
        //            //    case 2:
        //            //        break;
        //            //}
        //        }
        //    }

        //}

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
           // var selectenItem = (RadiosInfo)listRadioList.ItemsSource.SelectedItem;
           // listRadioList.ScrollTo(selectenItem);
            //  Debug.WriteLine("AudioPlayer.isoPlayState);" + AudioPlayer.isoPlayState);
        }

        private void topMenBar_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
        }

        private void topMenBar_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            //System.Windows.Shapes.Rectangle rg = sender as Rectangle;
            //TranslateTransform tf = new TranslateTransform();
            //tf.X = e.DeltaManipulation.Translation.X;
            //tf.Y = e.DeltaManipulation.Translation.Y;

            //rg.RenderTransform = tf;

            //e.Handled = true;
        }

        private void listRadioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectenItem = (RadiosInfo)listRadioList.SelectedItem;
                listRadioList.ScrollTo(selectenItem);

                AudioTrack selectAudioTrack = new AudioTrack(new Uri(selectenItem.URL, UriKind.Absolute), selectenItem.Name, selectenItem.NamePinyin, "", null, "", EnabledPlayerControls.Pause);
                //TODO：查找当前Select项在_PlayList中的index，然后给 isoCurrentTrack
                // var index=  _playList.BinarySearch(selectAudioTrack);
                //  _playList.Where(p => p.Source == selectAudioTrack.Source).ToList();

                //var a = from p in _playList
                //        where p.Source == selectAudioTrack.Source
                //        select new AudioTrack(
                //            p.Source,
                //            p.Title,
                //            p.Artist,p.Album,p.AlbumArt,p.Tag,p.PlayerControls

                //            );

                foreach (var item in _playList)
                {
                    if (item.Source == selectAudioTrack.Source)
                    {
                        selectAudioTrack = item;
                    }
                }

                int index = _playList.IndexOf(selectAudioTrack, 0);

                AppConfig.isoCurrentTrack = index;

                //TODO:播放当前选择项
                Debug.WriteLine("SelectItem Radio URL:" + selectenItem.URL);
                Debug.WriteLine("SelectenItem Radio Index:" + index);
                BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];// new AudioTrack(new Uri(selectenItem.URL, UriKind.Absolute), selectenItem.Name, null, null, null, "fd", EnabledPlayerControls.Pause);
                //BackgroundAudioPlayer.Instance.Volume = 1.0d;
            }
            catch (Exception ex)
            {
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
                PlayState playState = PlayState.Unknown;
                //System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":  Instance_PlayStateChanged- {0}", playState);
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
                this.timerr.Stop();
                BackgroundAudioPlayer.Instance.Stop();
                this.UpdateState(null, null);
                Debug.WriteLine("Stop_Click Play:" + AppConfig.isoCurrentTrack);

                FrameworkDispatcher.Update();
                MediaPlayer.Stop();
                FrameworkDispatcher.Update();
                MediaPlayer.Play(Song.FromUri("Snooze It!", new Uri("Audio/Void.wav", UriKind.Relative)));
                FrameworkDispatcher.Update();
                //if (DataCommunication.Read().ClearZunePlaylist)
                //{
                //    MediaPlayer.Play(Song.FromUri("Snooze It!", new Uri("Audio/Void.wav", UriKind.Relative)));
                //    FrameworkDispatcher.Update();
                //}
                MediaPlayer.Stop();
                FrameworkDispatcher.Update();

                Application.Current.Terminate();//退出应用程序
            }
            catch (Exception ex)
            {
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
                if (!BackgroundAudioPlayer.Instance.CanPause)
                {
                    return;
                }
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
                //BackgroundAudioPlayer.Instance.Play();

                //BackgroundAudioPlayer.Instance.Track = new AudioTrack(new Uri("mms://a1450.l11459845449.c114598.g.lm.akamaistream.net/D/1450/114598/v0001/reflector:45449", UriKind.Absolute), "SKY.FM", null, null, null, "fd", EnabledPlayerControls.Pause);

                BackgroundAudioPlayer.Instance.Track = _playList[AppConfig.isoCurrentTrack];
                //BackgroundAudioPlayer.Instance.Play();
                BackgroundAudioPlayer.Instance.Volume = 1.0d;

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
                UmengSDK.UmengAnalytics.TrackException(ex);
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //borderCenter.Margin = new Thickness(0, 0, 0, 0);
                //MoveAnimation.MoveTo(borderCenter, 120, 120, TimeSpan.FromSeconds(1.5), null);
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

                #region 动画

                //int index = 0;
                //DispatcherTimer timer = new DispatcherTimer();
                //timer.Tick += (a, w) =>
                //    {
                //        if (index == 0)
                //        {
                //            MoveAnimation.MoveTo(borderBottom, 102, 150, TimeSpan.FromSeconds(0.5), null);
                //        }
                //        else if (index == 1)
                //        {
                //            MoveAnimation.MoveTo(borderLeft, 50, 102, TimeSpan.FromSeconds(0.5), null);
                //        }
                //        else if (index == 2)
                //        {
                //            MoveAnimation.MoveTo(borderRight, 150, 50, TimeSpan.FromSeconds(0.5), null);
                //        }

                //        else if (index == 3)
                //        {
                //            MoveAnimation.MoveTo(borderTop, 50, 50, TimeSpan.FromSeconds(0.5), null);
                //        }
                //        else if (index == 4)
                //        {
                //            MoveAnimation.MoveTo(borderCenter, 100, 100, TimeSpan.FromSeconds(0.5), null);
                //        }
                //        else
                //        {
                //            timer.Stop();
                //            Debug.WriteLine("Move over!");
                //        }
                //        index++;
                //    };
                //timer.Interval = TimeSpan.FromSeconds(0.3);
                //timer.Start();

                #endregion 动画

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
                                }
                            });
                    });

                //SunshineStory.Begin();

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