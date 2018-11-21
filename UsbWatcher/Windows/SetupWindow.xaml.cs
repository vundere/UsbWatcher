using System;
using System.Windows;

namespace UsbWatcher
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        public SetupWindow()
        {
            InitializeComponent();
        }

        public void SetProgressBarMax(int maxValue)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                progressBar.Maximum = maxValue;
            }));
        }

        public void UpdateProgressBar(int linesProcessed)
        {


            this.Dispatcher.Invoke(new Action(() => 
            {
                progressBar.Value = linesProcessed;
            }));
        } 
    }
}
