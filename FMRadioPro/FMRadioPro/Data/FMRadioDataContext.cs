using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMRadioPro.Data
{
   public  class FMRadioDataContext:DataContext
    {
       public const string ConnectionString = "Data Source=isostore:/FMRadios.sdf";
       public Table<FMRadioItem> Rows;
       public FMRadioDataContext()
           : base(ConnectionString)
       { }
    }
}
