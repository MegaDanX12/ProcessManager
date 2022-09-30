using Microsoft.Win32.SafeHandles;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using System;
using System.Windows;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per HandlePropertiesWindow.xaml
    /// </summary>
    public partial class HandlePropertiesWindow : Window
    {
        private readonly ProcessPropertiesVM VM;

        public HandlePropertiesWindow(HandleInfo Info, ProcessPropertiesVM VM)
        {
            this.VM = VM;
            DataContext = Info;
            InitializeComponent();
        }

        private void OtherInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Tuple<SafeProcessHandle, HandleInfo> Parameters = new(VM.Handle, (HandleInfo)DataContext);
            VM.ShowHandleOtherPropertiesCommand.Execute(Parameters);
        }
    }
}