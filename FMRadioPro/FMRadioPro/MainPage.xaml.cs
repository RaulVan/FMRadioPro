using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FMRadioPro.Resources;
using System.Threading.Tasks;
using FMRadioPro.Data;
using Utility.Animations;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.Phone.BackgroundAudio;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FMRadioPro
{
    public partial class MainPage : PhoneApplicationPage
    {

        // AudioCategory 
        // 构造函数
        public MainPage()
        {
            InitializeComponent();
            BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged;
            
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
            this.btnStop.Click += btnStop_Click;
            this.listRadioList.SelectionChanged += listRadioList_SelectionChanged;
            this.topMenBar.ManipulationDelta += topMenBar_ManipulationDelta;
            this.topMenBar.ManipulationCompleted += topMenBar_ManipulationCompleted;

            topMenBar.Visibility = Visibility.Collapsed;

           
        }

        private void UpdateButton()
        {
 
        }


        void topMenBar_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
             
        }

        void topMenBar_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {

            Rectangle rg = sender as Rectangle;
            TranslateTransform tf = new TranslateTransform();
            tf.X = e.DeltaManipulation.Translation.X;
            tf.Y = e.DeltaManipulation.Translation.Y;

            rg.RenderTransform = tf;

            e.Handled = true;

        }

       

        void listRadioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectenItem =(RadiosInfo) listRadioList.SelectedItem;
            listRadioList.ScrollTo(selectenItem);
            //TODO:播放当前选择项
            Debug.WriteLine(selectenItem.URL);
            BackgroundAudioPlayer.Instance.Track = new AudioTrack(new Uri(selectenItem.URL, UriKind.Absolute), selectenItem.Name, null, null, null, "fd", EnabledPlayerControls.Pause);
            BackgroundAudioPlayer.Instance.Volume = 1.0d;

        }

        /// <summary>
        /// 播放状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            PlayState playState = PlayState.Unknown;
            try
            {
                playState = BackgroundAudioPlayer.Instance.PlayerState;
            }
            catch (InvalidOperationException)
            {
                playState = PlayState.Stopped;
            }

            switch (playState)
            {
                case PlayState.BufferingStarted:
                    break;
                case PlayState.BufferingStopped:
                    break;
                case PlayState.Error:
                    break;
                case PlayState.FastForwarding:
                    break;
                case PlayState.Paused:
                    break;
                case PlayState.Playing:
                   
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.Shutdown:
                    break;
                case PlayState.Stopped:
                    break;
                case PlayState.TrackEnded:
                    break;
                case PlayState.TrackReady:
                    break;
                case PlayState.Unknown:
                    break;
                default:
                    break;
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
            #endregion
        }
        /// <summary>
        /// 停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnStop_Click(object sender, RoutedEventArgs e)
        {
            BackgroundAudioPlayer.Instance.Stop();
        }
        /// <summary>
        /// 下一曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNext_Click(object sender, RoutedEventArgs e)
        {
            //BackgroundAudioPlayer.Instance.SkipNext();
        }
        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnPause_Click(object sender, RoutedEventArgs e)
        {
            BackgroundAudioPlayer.Instance.Pause();
        }
        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //BackgroundAudioPlayer.Instance.Play();

            BackgroundAudioPlayer.Instance.Track = new AudioTrack(new Uri("mms://a1450.l11459845449.c114598.g.lm.akamaistream.net/D/1450/114598/v0001/reflector:45449", UriKind.Absolute), "SKY.FM", null, null, null, "fd", EnabledPlayerControls.Pause);
            BackgroundAudioPlayer.Instance.Volume = 1.0d;
        }

        /// <summary>
        /// 上一曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnBack_Click(object sender, RoutedEventArgs e)
        {
           // BackgroundAudioPlayer.Instance.SkipPrevious();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //borderCenter.Margin = new Thickness(0, 0, 0, 0);
            //MoveAnimation.MoveTo(borderCenter, 120, 120, TimeSpan.FromSeconds(1.5), null);
           
        }



        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (PlayState.Playing==BackgroundAudioPlayer.Instance.PlayerState)
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


            int index = 0;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += (a, w) =>
                {
                    if (index == 0)
                    {
                      
                        MoveAnimation.MoveTo(borderBottom, 102, 150, TimeSpan.FromSeconds(0.5), null);
                    }
                    else if (index == 1)
                    {
                        MoveAnimation.MoveTo(borderLeft, 50, 102, TimeSpan.FromSeconds(0.5), null);
                    }
                    else if (index == 2)
                    {
                        MoveAnimation.MoveTo(borderRight, 150, 50, TimeSpan.FromSeconds(0.5), null);
                    }

                    else if (index == 3)
                    {
                        MoveAnimation.MoveTo(borderTop, 50, 50, TimeSpan.FromSeconds(0.5), null);
                    }
                    else if (index == 4)
                    {
                        MoveAnimation.MoveTo(borderCenter, 100, 100, TimeSpan.FromSeconds(0.5), null);
                    }
                    else
                    {
                        timer.Stop();
                        Debug.WriteLine("Move over!");
                    }
                    index++;
                };
            timer.Interval = TimeSpan.FromSeconds(0.3);
            timer.Start();

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


                        });
                });

            base.OnNavigatedTo(e);
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