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

namespace FMRadioPro
{
    public partial class MainPage : PhoneApplicationPage
    {

       // AudioCategory 
        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();
            //List<string> data=new List<string> ();
            //for (int i = 0; i < 100; i++)
            //{
            //    data.Add(i + "/Deanna 频道 test频道 test频道 test频道 test");
            //}
            //listRadioList.ItemsSource = data;
           // gridPanel.Width = Application.Current.Host.Content.ActualWidth * 2;

           
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
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