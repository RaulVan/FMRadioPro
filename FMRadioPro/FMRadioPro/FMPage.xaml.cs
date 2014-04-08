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
using FMRadioPro.Data;

namespace FMRadioPro
{
    public partial class FMPage : PhoneApplicationPage
    {

        double frequency;
        bool flag = false;

        FMRadio fmRadio;
        public FMPage()
        {
            InitializeComponent();

            //TODO:启动后加载上次退出时的频道，没有就默认


            this.loopSelector1.DataSource = new IntLoopingDataSource() { MinValue = 87, MaxValue = 108, SelectedItem = AppConfig.isoCurrentFMFrequency1 };
            this.loopSelector2.DataSource = new IntLoopingDataSource() { MinValue = 0, MaxValue = 9, SelectedItem = AppConfig.isoCurrentFMFrequency2 };
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
            double fre = Str2Fre(fre1, fre2);
            if (fre<87.5 || fre>108)
            {
                return;
            }

            try
            {
                fmRadio = FMRadio.Instance;
                fmRadio.PowerMode = RadioPowerMode.On;
                fmRadio.CurrentRegion = RadioRegion.Europe;
            }
            catch (Exception )
            {
                 MessageBox.Show("确保您的手机可以正常使用收音机功能!", "提示", MessageBoxButton.OK);
                return;
            }

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

            fmRadio.Frequency = fre;

            frequency = fre;
            flag = true;
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

        /// <summary>
        /// string to double (Frequency)
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        private double Str2Fre(string str1,string str2)
        {
            return double.Parse(str1) + (double.Parse(str2) / 10);
        }



        void FMPage_Loaded(object sender, RoutedEventArgs e)
        {

            //FMRadioModel model = new FMRadioModel();
            //model.SelectRadio();
            //foreach (var item in model.Items)
            //{
            //    Debug.WriteLine(item.Frequency);
            //}
        }

        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void btnAddRadio_Click(object sender, RoutedEventArgs e)
        {
            if (flag)
            {
                FMRadioItem radioitem = new FMRadioItem();
                radioitem.Id = Guid.NewGuid();
                radioitem.Frequency = frequency;
                FMRadioModel model = new FMRadioModel();
                model.AddRadio(radioitem); 
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AppConfig.isoCurrentFMFrequency1 = int.Parse(loopSelector1.DataSource.SelectedItem.ToString());
            AppConfig.isoCurrentFMFrequency2 = int.Parse(loopSelector2.DataSource.SelectedItem.ToString());
            base.OnNavigatedFrom(e);
        }
    }
}