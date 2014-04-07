using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMRadioPro.Data
{
   public class FMRadioModel
    {

       private bool isAddToDB = true;
       public FMRadioModel()
       {
           this.Items = new ObservableCollection<FMRadioItem>();
           this._DB = new FMRadioDataContext();
           if (!_DB.DatabaseExists())
           {
               _DB.CreateDatabase();
           }
       }

       public ObservableCollection<FMRadioItem> Items
       {
           get;
           private set;
       }

       private static FMRadioModel _Instance;
       public static FMRadioModel GetInstance()
        {
            if (_Instance == null)
                _Instance = new FMRadioModel();
            return _Instance;
        }

       private FMRadioDataContext _DB;

       public FMRadioDataContext DB
       {
           get { return _DB; }
           set { _DB = value; }
       }
       /// <summary>
       /// 新增
       /// </summary>
       /// <param name="item"></param>
       public void AddRadio(FMRadioItem item)
       {
           try
           {
               this.Items.Add(item);
               _DB.Rows.InsertOnSubmit(item);
               _DB.SubmitChanges();
           }
           catch (Exception ex)
           {
               Debug.WriteLine("AddRadio"+ex.Message);
           }
       }
       /// <summary>
       /// 删除
       /// </summary>
       /// <param name="item"></param>
       public void DeleteRadio(FMRadioItem item)
       {
           try
           {
               if (_DB.DatabaseExists())
               {
                   _DB.Rows.DeleteOnSubmit(item);
                   _DB.SubmitChanges();
                   this.Items.Remove(item);
               }
           }
           catch (Exception ex)
           {
               Debug.WriteLine("DeleteRadio" + ex.Message);
           }
       }

       public void SelectRadio()
       {
           try
           {
               if (isAddToDB)
               {
                   if (_DB.DatabaseExists())
                   {
                       IEnumerator<FMRadioItem> en = _DB.Rows.GetEnumerator();
                       while (en.MoveNext())
                       {
                           this.Items.Add(en.Current);
                       }
                       isAddToDB = false;
                   }
               }
           }
           catch (Exception ex)
           {
               Debug.WriteLine("SelectRadio" + ex.Message);
           }
       }
    }
}
