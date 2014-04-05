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

namespace FMRadioPro
{
    public partial class FMPage : PhoneApplicationPage
    {
        FMRadio fmRadio;
        public FMPage()
        {
            InitializeComponent();
            btnPlay.Click += btnPlay_Click;
            this.Loaded += FMPage_Loaded;
        }

        void FMPage_Loaded(object sender, RoutedEventArgs e)
        {
            fmRadio = FMRadio.Instance;
            fmRadio.PowerMode = RadioPowerMode.On;
        }

        void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += (a,s) =>
                {
                    Debug.WriteLine(fmRadio.SignalStrength.ToString());

                };

        }


    }
}