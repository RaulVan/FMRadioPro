using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

//代码参照 http://www.cnblogs.com/webabcd/archive/2013/11/26/3442599.html
namespace FMRadioPro.Data
{
    public class RadiosData
    {
        /// <summary>
        /// 按拼音分组数据
        /// </summary>
        private static readonly string _groupLetters = "#abcdefghijklmnopqrstuvwxyz";

        private static List<RadiosInfoInGroup> _data;
        private static List<RadiosInfo> _radios;

        public static List<RadiosInfo> GetRadioData()
        {
            GetData();
            return _radios;
        }
        /// <summary>
        /// 获取全部电台数据
        /// </summary>
        /// <returns></returns>
        public static List<RadiosInfoInGroup> GetData()
        {
            if (_data==null)
            {
                _data = new List<RadiosInfoInGroup>();
                _radios = new List<RadiosInfo>();
                Dictionary<string, RadiosInfoInGroup> groups = new Dictionary<string, RadiosInfoInGroup>();
                foreach (char c in _groupLetters.ToUpper())
                {
                    RadiosInfoInGroup group = new RadiosInfoInGroup(c.ToString());
                    _data.Add(group);
                    groups[c.ToString()] = group;
                }

                //解析并获取数据
                //TODO：使用数据库，目前使用文本
                StreamResourceInfo resource = App.GetResourceStream(new Uri("Resources/RadioInfo.txt", UriKind.Relative));
                StreamReader sr = new StreamReader(resource.Stream);
                string line = sr.ReadLine();
                while (!string.IsNullOrWhiteSpace(line))
                {
                    var ary = line.Split('=');
                    var radioInfo = new RadiosInfo { Name = ary[0], URL = ary[1], NamePinyin = ary[2] };
                    _radios.Add(radioInfo);
                    groups[RadiosInfo.GetNameFirstPinyinKey(radioInfo)].Add(radioInfo);
                    line = sr.ReadLine();

                }

                   resource = App.GetResourceStream(new Uri("Resources/Text1.txt", UriKind.Relative));
                 sr = new StreamReader(resource.Stream);
                 line = sr.ReadLine();
                 while (!string.IsNullOrWhiteSpace(line))
                 {
                     var ary = line.Split('=');
                     var radioInfo = new RadiosInfo { Name = ary[0], URL = ary[1] };
                     _radios.Add(radioInfo);
                     groups[RadiosInfo.GetNameFirstPinyinKey(radioInfo)].Add(radioInfo);
                     line = sr.ReadLine();

                 }
            }
            return _data;
        }

        

        /// <summary>
        /// 获取包含指定关键字的数据
        /// </summary>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        public static List<RadiosInfoInGroup> GetData(string searchKey)
        {
            searchKey = searchKey.ToUpper();
            List<RadiosInfoInGroup> result = new List<RadiosInfoInGroup>();
            List<RadiosInfoInGroup> data = GetData();
            foreach (RadiosInfoInGroup rig in data)
            {
                List<RadiosInfo> radioData = rig.Where(p => p.Name.Contains(searchKey) || p.NamePinyin.Contains(searchKey)).ToList();
                if (radioData!=null)
                {
                    RadiosInfoInGroup resultRadio = new RadiosInfoInGroup(rig.Index);
                    resultRadio.AddRange(radioData);
                    result.Add(resultRadio);
                }
            }
            return result;
        }

    }
}
