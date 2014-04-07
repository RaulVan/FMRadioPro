using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FMRadioPro.Utilities
{
    public class LongListSelectorEx : LongListSelector  
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            try
            {
                return base.MeasureOverride(availableSize);
            }
            catch (ArgumentException)
            {
                return base.MeasureOverride(availableSize);
            }
        }
    }
}
