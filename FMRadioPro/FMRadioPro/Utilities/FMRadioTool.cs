using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMRadioPro.Utilities
{
    public static class FMRadioTool
    {
        /// <summary>
        /// 停止电台播放
        /// <para>UserAction</para>
        /// </summary>
        public static void StopRadioPlay()
        {
            FrameworkDispatcher.Update();
            MediaPlayer.Stop();
            FrameworkDispatcher.Update();
            MediaPlayer.Play(Song.FromUri("Snooze It!", new Uri("Audio/Void.wav", UriKind.Relative)));
            FrameworkDispatcher.Update();

            MediaPlayer.Stop();
            FrameworkDispatcher.Update();

        }


    }
}
