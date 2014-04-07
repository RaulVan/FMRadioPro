using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices.Radio;
using System.Windows.Threading;
using System.Diagnostics;
using FMRadioPro.Utilities;

namespace FMRadioPro
{
    public partial class FMPage : PhoneApplicationPage
    {
        
        FMRadio fmRadio;
        public FMPage()
        {
            InitializeComponent();

            //TODO:启动后加载上次退出时的频道，没有就默认

            this.loopSelector1.DataSource = new IntLoopingDataSource() { MinValue = 87, MaxValue = 108, SelectedItem = 87 };
            this.loopSelector2.DataSource = new IntLoopingDataSource() { MinValue = 0, MaxValue = 9, SelectedItem = 1 };
            this.loopSelector1.DataSource.SelectionChanged += DataSource_SelectionChanged;
            this.loopSelector2.DataSource.SelectionChanged+=DataSource_SelectionChanged2;

            btnPlay.Click += btnPlay_Click;
            this.Loaded += FMPage_Loaded;
        }

        void DataSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           Debug.WriteLine("====="+ loopSelector1.DataSource.SelectedItem.ToString());
           Debug.WriteLine(loopSelector2.DataSource.SelectedItem.ToString());
           this.loopSelector1.IsExpanded = false;
           Play(loopSelector1.DataSource.SelectedItem.ToString(), loopSelector2.DataSource.SelectedItem.ToString());
        }
        private void DataSource_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("====="+loopSelector1.DataSource.SelectedItem.ToString());
            Debug.WriteLine(loopSelector2.DataSource.SelectedItem.ToString());
            this.loopSelector2.IsExpanded = false;
            Play(loopSelector1.DataSource.SelectedItem.ToString(), loopSelector2.DataSource.SelectedItem.ToString());
        }

        private void Play(string fre1,string fre2)
        {
            double fre = double.Parse(fre1) +(double.Parse(fre2) / 10);
            if (fre<87.5 || fre>108)
            {
                return;
            }
            fmRadio.Frequency = fre;

            //TODO:信号强度
            //this.Dispatcher.BeginInvoke(() =>
            //{
            //    DispatcherTimer timer = new DispatcherTimer();
            //    timer.Tick += (a, s) =>
            //        {
            //            Debug.WriteLine(fmRadio.SignalStrength.ToString());

            //        };
            //    timer.Interval = TimeSpan.FromSeconds(1);
            //    timer.Start();
            //});
        }

        void FMPage_Loaded(object sender, RoutedEventArgs e)
        {
            fmRadio = FMRadio.Instance;
            fmRadio.PowerMode = RadioPowerMode.On;
            fmRadio.CurrentRegion = RadioRegion.Europe;

            try
            {
                if (fmRadio.SignalStrength == 0)
                {
                }
            }
            catch (Exception)
            {
                    MessageBox.Show("确保插入耳机，正常使用收音机功能!", "提示", MessageBoxButton.OK);
                    return;
                
                
            }
           
        }

        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
           
        }


    }
}