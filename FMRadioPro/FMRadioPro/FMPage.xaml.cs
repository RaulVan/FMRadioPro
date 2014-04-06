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
        private string strFrequency;

        FMRadio fmRadio;
        public FMPage()
        {
            InitializeComponent();
            this.loopSelector1.DataSource = new IntLoopingDataSource() { MinValue = 87, MaxValue = 108, SelectedItem = 87 };
            this.loopSelector2.DataSource = new IntLoopingDataSource() { MinValue = 0, MaxValue = 10, SelectedItem = 1 };

            btnPlay.Click += btnPlay_Click;
            this.Loaded += FMPage_Loaded;
        }

        void FMPage_Loaded(object sender, RoutedEventArgs e)
        {
            //fmRadio = FMRadio.Instance;
            //fmRadio.PowerMode = RadioPowerMode.On;
        }

        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //DispatcherTimer timer = new DispatcherTimer();
            //timer.Tick += (a,s) =>
            //    {
            //        Debug.WriteLine(fmRadio.SignalStrength.ToString());

            //    };

        }


    }
}