using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Net;
using Microsoft.Phone.Net.NetworkInformation;
using System.Windows;

namespace FMRadioPro
{
   public  class AppConfig
    {
       /// <summary>
       /// 友盟统计API Key
       /// </summary>
       public static string AppKey = "530af5be56240b7e95046e75";

       /// <summary>
       /// DEBUG是使用key
       /// </summary>
       public static string DebugAppKey = "5313e59056240b7a8a1ab50e";
       /// <summary>
        /// 当前播放曲目
       /// </summary>
       public static int isoCurrentTrack
       {
           get
           {
               return IsolatedStorageSettings.ApplicationSettings.Contains("isoCurrentTrack") ? (int)IsolatedStorageSettings.ApplicationSettings["isoCurrentTrack"] : 0;
           }
           set
           {
               IsolatedStorageSettings.ApplicationSettings["isoCurrentTrack"] = value;
               IsolatedStorageSettings.ApplicationSettings.Save();
           }
       }

       public static List<AudioTrack> isoPlayTrack
       {
           get
           {
               return IsolatedStorageSettings.ApplicationSettings.Contains("isoPlayTrack") ? (List<AudioTrack>)IsolatedStorageSettings.ApplicationSettings["isoPlayTrack"] : null;

           }
           set
           {
               IsolatedStorageSettings.ApplicationSettings["isoPlayTrack"] = value;
               IsolatedStorageSettings.ApplicationSettings.Save();
           }
       }
      

       public string GetNetwork()
       {
           return DeviceNetworkInformation.CellularMobileOperator.ToString();
           //var info = Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType;
           //switch (info)
           //{
           //    case NetworkInterfaceType.Wireless80211:
           //        return "WiFi";
           //    default:
           //        return "Other";
           //}
       }

       public static int isoCurrentFMFrequency1
       {
           get
           {
               return IsolatedStorageSettings.ApplicationSettings.Contains("isoCurrentFMFrequency1") ? (int)IsolatedStorageSettings.ApplicationSettings["isoCurrentFMFrequency1"] : 101;
           }
           set
           {
               IsolatedStorageSettings.ApplicationSettings["isoCurrentFMFrequency1"] = value;
               IsolatedStorageSettings.ApplicationSettings.Save();
           }
       }

       public static int isoCurrentFMFrequency2
       {
           get
           {
               return IsolatedStorageSettings.ApplicationSettings.Contains("isoCurrentFMFrequency2") ? (int)IsolatedStorageSettings.ApplicationSettings["isoCurrentFMFrequency2"] : 1;
           }
           set
           {
               IsolatedStorageSettings.ApplicationSettings["isoCurrentFMFrequency2"] = value;
               IsolatedStorageSettings.ApplicationSettings.Save();
           }
       }

       public int GetSecleFactor()
       {
           int height=0;
           switch (Application.Current.Host.Content.ScaleFactor)
           {
               case 100:
               case 150:
                   height = 854;
                   break;
               case 160:
                   height = 800;
                   break;
              
           }
           return height;
       }

    }
}
