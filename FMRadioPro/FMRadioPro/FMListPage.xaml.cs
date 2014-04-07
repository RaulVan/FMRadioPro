using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FMRadioPro.Data;
using System.Collections.ObjectModel;


namespace FMRadioPro
{
    public partial class FMListPage : PhoneApplicationPage
    {
        public FMListPage()
        {
            InitializeComponent();

        }
        private void Select()
        {
            FMRadioModel model = new FMRadioModel();
            model.SelectRadio();
            ObservableCollection<FMRadioItem> radioitems = model.Items;
            //TODO:显示
            listRadio.ItemsSource = radioitems;
        }

    }
}