using System.IO;
using System.Windows;

namespace UsbWatcher
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            GetStatus();

            DataContext = MainWindow.userSettings;

        }

        private void GetStatus()
        {
            if (!File.Exists(Constants.dbDefaultLocation))
            {
                labelDbStatus.Content = "NOT FOUND";
            }
            else
            {
                labelDbStatus.Content = "OK";
            }

            if (!File.Exists(Constants.usbIdFileDefaultLocation))
            {
                labelFileStatus.Content = "NOT FOUND";
            }
            else
            {
                labelFileStatus.Content = "OK";
            }

            labelLastDownload.Content = DataHandler.GetLast(true);

        }

        private void ButtonUpdateDb_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.FileToDb();
        }

        private void ButtonUpdateIdFile_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.DownloadIdFile();
        }
    }
}
