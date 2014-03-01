using Microsoft.Phone.BackgroundAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioPlaybackAgent
{
   public class PlayStateEventArgs:EventArgs
    {
       public PlayState playState { get; set; }
      
    }
}
