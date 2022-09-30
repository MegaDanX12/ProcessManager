using Microsoft.Win32.SafeHandles;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class ImageInfoCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public ImageInfoCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            Tuple<SafeProcessHandle, string> ParameterTuple = Parameter as Tuple<SafeProcessHandle, string>;
            PEHeaderInfo HeaderInfo = NativeHelpers.GetPeHeaderInfo(ParameterTuple.Item1, ParameterTuple.Item2);
            if (HeaderInfo != null)
            {
                PeHeaderInfoVM HeaderInfoVM = new(HeaderInfo);
                PeHeaderInfoWindow Window = new(HeaderInfoVM);
                Window.ShowDialog();
            }
            else
            {
                MessageBox.Show(Properties.Resources.PeParsingErrorMessageText, Properties.Resources.PeParsingErrorMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}