using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//代码参照 http://www.cnblogs.com/webabcd/archive/2013/11/26/3442599.html

namespace FMRadioPro.Data
{
   public   class RadiosInfo
    {
       /// <summary>
       /// 名称
       /// </summary>
       public string Name { get; set; }
       
       /// <summary>
       /// URL电台地址
       /// </summary>
       public string URL { get; set; }

       /// <summary>
       /// 名称拼音
       /// </summary>
       public string NamePinyin { get; set; }

       public static string GetNameFirstPinyinKey(RadiosInfo radioInfo)
       {
           char index = char.ToUpper(radioInfo.NamePinyin[0]);
           if (index<'A'||index>'Z')
           {
               index = '#';
           }
           return index.ToString();

       }

    }
}
