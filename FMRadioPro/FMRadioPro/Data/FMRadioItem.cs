using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data.Linq.Mapping;

namespace FMRadioPro.Data
{
    public class FMRadioItem:INotifyPropertyChanged,INotifyPropertyChanging
    {
        //public FMRadioItem()
        //{ }
        public FMRadioItem(double frequency)
        {
            _id = Guid.NewGuid();
            _frequency = frequency;
        }

        private Guid _id;
        [Column(IsPrimaryKey = true, CanBeNull = false, IsDbGenerated = false, AutoSync = AutoSync.Default)]
        public Guid Id
        {
            get { return _id; }
            set
            {
                NotifyPropertyChanged("Id");
                _id = value;
                NotifyPropertyChanged("Id");
            }

        }
        private double _frequency;
        [Column]
        public double Frequency
        {
            get { return _frequency; }
            set 
            {
                NotifyPropertyChanged("Frequency");
                _frequency = value;
                NotifyPropertyChanged("Frequency");
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Implementation of INotifyPropertyChanging

        public event PropertyChangingEventHandler PropertyChanging;

        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion 

    }
}
