using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Utility.Animations;
using System.Windows.Threading;
using System.Threading;

namespace FMRadioPro
{
    public partial class HeartPage : PhoneApplicationPage
    {
        double Width = Application.Current.Host.Content.ActualWidth;

        public HeartPage()
        {
            InitializeComponent();
            this.DoubleTap += HeartPage_DoubleTap;
            imageL.Tap += imageL_Tap;
            imageR.Tap += imageR_Tap;
        }

        void imageR_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MoveAnimation.MoveTo(imageL, -Width / 2, 0, TimeSpan.FromSeconds(1), null);
            MoveAnimation.MoveTo(imageR, Width / 2, 0, TimeSpan.FromSeconds(1), null);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += ((a, s) =>
            {
                //base.OnBackKeyPress(e);
                //Application.Current.Terminate();
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                timer.Stop();
            });
            timer.Interval = TimeSpan.FromSeconds(.2);
            timer.Start();
        }

        void imageL_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MoveAnimation.MoveTo(imageL, -Width / 2, 0, TimeSpan.FromSeconds(1), null);
            MoveAnimation.MoveTo(imageR, Width / 2, 0, TimeSpan.FromSeconds(1), null);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += ((a, s) =>
            {
                //base.OnBackKeyPress(e);
                //Application.Current.Terminate();
                NavigationService.Navigate(new Uri("/FMPage.xaml", UriKind.Relative));
                timer.Stop();
            });
            timer.Interval = TimeSpan.FromSeconds(0.2);
            timer.Start();
        }

        void HeartPage_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            

            MoveAnimation.MoveTo(imageL, -Width/2, 0, TimeSpan.FromSeconds(1), null);
            MoveAnimation.MoveTo(imageR, Width/2, 0, TimeSpan.FromSeconds(1), null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            MoveAnimation.MoveTo(imageL, -imageL.ActualWidth, 0, TimeSpan.FromSeconds(1), null);
            MoveAnimation.MoveTo(imageR, imageR.ActualWidth, 0, TimeSpan.FromSeconds(1), null);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            MoveAnimation.MoveTo(imageL, Width / 2 - imageL.Width, 0, TimeSpan.FromSeconds(1), null);
            MoveAnimation.MoveTo(imageR, -Width / 2 + imageR.Width, 0, TimeSpan.FromSeconds(1), null);

            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
           // e.Cancel = true;
           // MoveAnimation.MoveTo(imageL, Width/2-imageL.Width, 0, TimeSpan.FromSeconds(1), null);
           // MoveAnimation.MoveTo(imageR, -Width/2+imageR.Width, 0, TimeSpan.FromSeconds(1), null);

           //// Thread.Sleep(2);

           // DispatcherTimer timer = new DispatcherTimer();
           // timer.Tick += ((a, s) =>
           // {
           //     //base.OnBackKeyPress(e);
           //     Application.Current.Terminate();
           //     timer.Stop();
           // });
           // timer.Interval = TimeSpan.FromSeconds(2);
           // timer.Start();
            
        }
    }
}