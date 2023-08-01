using ProcessManager.InfoClasses.HandleSpecificInfo;
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
using System.Windows.Shapes;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per SectionInfoWindow.xaml
    /// </summary>
    public partial class SectionInfoWindow : Window
    {
        public SectionInfoWindow(SectionInfo Info)
        {
            DataContext = Info;
            InitializeComponent();
        }
    }
}