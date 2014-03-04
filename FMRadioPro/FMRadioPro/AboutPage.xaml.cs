using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Marketplace;
using UmengSDK;

namespace FMRadioPro
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
            emailLink.Click += emailLink_Click;
            btnOpenAliPay.Click += btnOpenAliPay_Click;
        }

        void btnOpenAliPay_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://me.alipay.com/rauls", UriKind.Absolute);
            task.Show();
        }

        void emailLink_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask ect = new EmailComposeTask();
            ect.To = "raulgblanco@live.com";
            ect.Subject = "FM Radio Pro 电台反馈";
            ect.Show();
        }

        private void btnOpenMarket_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask task = new MarketplaceReviewTask();
            task.Show();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UmengAnalytics.TrackPageEnd("AboutPage");
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UmengAnalytics.TrackPageStart("AboutPage");
        }
    }
}