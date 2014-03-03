﻿using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMRadioPro
{
   public  class AppConfig
    {
       /// <summary>
       /// 友盟统计API Key
       /// </summary>
       public static string AppKey = "530af5be56240b7e95046e75";
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

      

    }
}
