using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//代码参照 http://www.cnblogs.com/webabcd/archive/2013/11/26/3442599.html

namespace FMRadioPro.Data
{
    public class RadiosInfoInGroup : List<RadiosInfo>
    {
        public RadiosInfoInGroup(string index)
        {
            Index = index;
        }
        /// <summary>
        /// 分组拼音首字母
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// 是否有Item
        /// </summary>
        public bool HasItems { get { return Count > 0; } }
    }
}
