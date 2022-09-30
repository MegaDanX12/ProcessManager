using System.Windows;
using System.Windows.Input;

namespace ProcessManager.Views
{
    /// <summary>
    /// Logica di interazione per FindWindowInfoWindow.xaml
    /// </summary>
    public partial class FindWindowInfoWindow : Window
    {
        /// <summary>
        /// ID del processo a cui appartiene la finestra sopra cui il puntatore si trova.
        /// </summary>
        public uint? PID { get; private set; }

        public FindWindowInfoWindow()
        {
            InitializeComponent();
        }

        private void FindWindowInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers is ModifierKeys.None)
            {
                if (e.Key is Key.Enter)
                {
                    PID = NativeHelpers.GetOwnerProcessIDOfWindowUnderCursor();
                    if (PID.HasValue)
                    {
                        DialogResult = true;
                    }
                    else
                    {
                        DialogResult = false;
                    }
                }
                else if (e.Key is Key.Escape)
                {
                    DialogResult = false;
                }
                Close();
            }
        }
    }
}