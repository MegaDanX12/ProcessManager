﻿using Microsoft.Win32.SafeHandles;
using ProcessManager.Models;
using ProcessManager.ViewModels;
using ProcessManager.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Commands.ProcessPropertiesWindowCommands
{
    public class ShowFilePropertiesCommand : ICommand
    {
        private readonly ProcessPropertiesVM VM;

        public ShowFilePropertiesCommand(ProcessPropertiesVM VM)
        {
            this.VM = VM;
        }

        public bool CanExecute(object Parameter)
        {
            return true;
        }

        public void Execute(object Parameter)
        {
            if (!VM.ShowModuleProperties(Parameter as ModuleInfo))
            {
                _ = MessageBox.Show(Properties.Resources.ModulePropertiesWindowShowErrorMessage, Properties.Resources.ModulePropertiesWindowShowErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}