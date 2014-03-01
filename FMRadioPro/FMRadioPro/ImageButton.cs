using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FMRadioPro
{
    public class ImageButton :Button
    {
        public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(ImageButton),
            null);

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
       
    }
}
