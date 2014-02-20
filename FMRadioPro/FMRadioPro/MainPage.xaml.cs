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
using Microsoft.Devices.Radio;

namespace FMRadioPro
{
    public partial class MainPage : PhoneApplicationPage
    {
        FMRadio radio;

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
            listRadioList.Visibility = Visibility.Collapsed;

            this.listRadioList.SelectionChanged += listRadioList_SelectionChanged;

        }

        void listRadioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectenItem = listRadioList.SelectedItem as RadiosInfo;
            listRadioList.ScrollTo(selectenItem);
            //TODO:播放当前本地电台

          //  radio.Frequency=
        }

        /// <summary>
        /// 播放状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            PlayStateChangedEventArgs newEventArgs = (PlayStateChangedEventArgs)e;
            
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.BufferingStarted:
                    //TODO:缓存进度条
                    Debug.WriteLine("11ing。。。。");
                    break;
                case PlayState.BufferingStopped:
                    //TODO:停止缓充
                    Debug.WriteLine("end。。。。。。。");
                    break;
                case PlayState.Error:
                    break;
                case PlayState.FastForwarding:
                    break;
                
                case PlayState.Playing:
                    btnPause.Visibility = Visibility.Visible;
                    btnPlay.Visibility = Visibility.Collapsed;
                    break;
                case PlayState.Rewinding:
                    break;
                case PlayState.Shutdown:
                    //TODO:应用退出提示是否继续后台播放，否，停止播放
                    break;
                case PlayState.Paused:
                case PlayState.Stopped:
                    btnPause.Visibility = Visibility.Collapsed;
                    btnPlay.Visibility = Visibility.Visible;
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
            if (BackgroundAudioPlayer.Instance.Track!=null)
            {
                //TODO:显示当前播放内容
            }
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
            BackgroundAudioPlayer.Instance.SkipNext();
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
            BackgroundAudioPlayer.Instance.Play();
        }

        /// <summary>
        /// 上一曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnBack_Click(object sender, RoutedEventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipPrevious();
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

           

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// 本地电台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnLocaFM_Click(object sender, RoutedEventArgs e)
        {
            //TODO:加载本地电台数据
            //结束网络电台播放
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
            listRadioList.Visibility = Visibility.Visible;
            
            radio = FMRadio.Instance;
            radio.CurrentRegion = RadioRegion.Europe;//除美国和日本外
            
        }

        /// <summary>
        /// 网络电台
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInterFM_Click(object sender, RoutedEventArgs e)
        {
            listRadioList.Visibility = Visibility.Collapsed;
           
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