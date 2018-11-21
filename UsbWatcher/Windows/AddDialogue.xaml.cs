using System.Windows;

namespace UsbWatcher
{
    /// <summary>
    /// Interaction logic for AddDialogue.xaml
    /// </summary>
    public partial class AddDialogue : Window
    {
        public AddDialogue(Device device = null)
        {
            InitializeComponent();

            if (device != null)
            {
                textBoxName.Text = device.Name;
                textBoxPid.Text = device.PID;
                textBoxVid.Text = device.VID;
                textBoxPnpId.Text = device.PnPID;

                textBoxSuggestedMfg.Text = DataHandler.GetVendor(device.VID);
                textBoxSuggestedProd.Text = DataHandler.GetProduct(device.PID, device.VID);

                DataContext = MainWindow.userSettings;

            }
        }

        private void ButtonAddDialogueOk_Click(object sender, RoutedEventArgs e)
        {

            DialogResult = true;
        }

        private void ButtonAddDialogueCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
