using ProcessManager.ViewModels;
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
    /// Logica di interazione per MitigationPoliciesDetailsWindow.xaml
    /// </summary>
    public partial class MitigationPoliciesDetailsWindow : Window
    {
        public MitigationPoliciesDetailsWindow(ProcessPropertiesVM VM)
        {
            DataContext = VM;
            InitializeComponent();
        }

        private void PoliciesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            string EnabledSettings = ((ProcessPropertiesVM)DataContext).GeneralInfo.EnabledSettings[(string)PoliciesListBox.SelectedItem];
            string[] Settings = EnabledSettings.Split(',');
            string TrimmedSetting;
            foreach (string setting in Settings)
            {
                TrimmedSetting = setting.TrimEnd(' ');
                sb.Append(TrimmedSetting).Append("\r\n");
            }
            PolicyDetailsTextBox.Text = sb.ToString();
        }
    }
}