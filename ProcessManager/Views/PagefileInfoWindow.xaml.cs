using ProcessManager.Models;
using ProcessManager.WMI;
using System;
using System.Globalization;
using System.Management;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per PagefileInfoWindow.xaml
    /// </summary>
    public partial class PagefileInfoWindow : Window
    {
        /// <summary>
        /// Azione da eseguire.
        /// </summary>
        private readonly string Action;
        public PagefileInfoWindow(string Action, PageFileInfo Info = null)
        {
            this.Action = Action;
            DataContext = Info;
            InitializeComponent();
            if (Info is not null)
            {
                string Filepath = Info.PageFilePath.Insert(Info.PageFilePath.IndexOf('\\'), "\\");
                ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_PageFileSetting WHERE Name =\"" + Filepath + "\"");
                ManagementObjectCollection Collection = Searcher.Get();
                foreach (ManagementObject file in Collection)
                {
                    uint InitialSize = (uint)file["InitialSize"];
                    uint MaximumSize = (uint)file["MaximumSize"];
                    InitialSizeTextBox.Text = InitialSize.ToString("D0", CultureInfo.CurrentCulture);
                    MaximumSizeTextBox.Text = MaximumSize.ToString("D0", CultureInfo.CurrentCulture);
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            switch (Action)
            {
                case "ChangeSize":
                    PageFileInfo Info = (PageFileInfo)DataContext;
                    DialogResult = Info.ChangeSize(Convert.ToUInt32(InitialSizeTextBox.Text, CultureInfo.CurrentCulture), Convert.ToUInt32(MaximumSizeTextBox.Text, CultureInfo.CurrentCulture));
                    break;
                case "CreateNew":
                    DialogResult = WMISystemInfoMethods.CreatePagefile(FilenameTextBox.Text, Convert.ToUInt32(InitialSizeTextBox.Text, CultureInfo.CurrentCulture), Convert.ToUInt32(MaximumSizeTextBox.Text, CultureInfo.CurrentCulture));
                    break;
            }
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}