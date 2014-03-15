using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Net;
using Microsoft.Phone.Net.NetworkInformation;

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

    }
}
