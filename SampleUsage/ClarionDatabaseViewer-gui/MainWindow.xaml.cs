using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClarionDatConnector;

namespace ClarionDatabaseViewer_gui
{
    /// <summary>
    /// Sample Usage Application for the ClarionDatConnector library. It creates a gui with all the data in an excel-like view
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ClarionFileData clarionFile = new ClarionFileData("INV.DAT");
            var k = clarionFile.GetData();
            //var i = clarionFile.ClarionData.DefaultView;
            this.DataContext = clarionFile.ClarionData;
            InitializeComponent();
        }

    }
}
