using System;
using System.Management;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using Config.Net;
using System.IO;

namespace UsbWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields

        public static IUserSettings userSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.SettingsPath).Build();

        private readonly BackgroundWorker worker = new BackgroundWorker();

        public Device currentSelectedDevice; 

        #endregion Fields

        #region Constructor

        public MainWindow()
        {
            SetupApplication();
            InitializeComponent();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker1_DoWork;
            worker.RunWorkerAsync();

            DataContext = userSettings;
        }
        
        #endregion Constructor

        #region Methods

        private void UpdateTree(EventType eventType, string identifier, List<string> data)
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                string Timestamp = DateTime.Now.ToString("HH:mm:ss");

                TreeViewItem treeItem = new TreeViewItem
                {
                    Header = $"{Timestamp}: {eventType} {identifier}"
                };
                foreach (var entry in data)
                {
                    treeItem.Items.Add(new TreeViewItem() { Header = entry });
                }

                treeEvents.Items.Add(treeItem);
            }));
        }

        public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
        }

        private void SetupApplication()
        {

            
            if (!File.Exists(Constants.usbIdFileDefaultLocation))
            {

                try
                {
                    DataHandler.DownloadIdFile();
                }
                catch (Exception)
                {

                    string message = $"Failed to download id file. Please download it manually at {Constants.usbIdFileUrl}.";
                    MessageBoxResult result = MessageBox.Show(this, message, "Error!", MessageBoxButton.OK);
                }
            }
            
            if (File.Exists(Constants.usbIdFileDefaultLocation) && !File.Exists(Constants.dbDefaultLocation))
            {
                DataHandler.FileToDb();
            }

            userSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.SettingsPath).Build();
            DataContext = userSettings;

        }

        #endregion Methods

        #region Events

        private void ButtonAddKnown_Click(object sender, RoutedEventArgs e)
        {
            AddDialogue dlg;
            dlg = new AddDialogue(currentSelectedDevice)
            {
                Owner = this,
                ShowInTaskbar = false,
            };

            dlg.ShowDialog();
        }

        private void ButtonRemoveKnown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TreeEvents_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem node = treeEvents.SelectedItem as TreeViewItem;
            string deviceId = "";
            string pnpDeviceId = "";
            string description = "";
            Device newDevice = new Device();

            if (node != null)
            {
                TreeViewItem parentNodeControl = GetSelectedTreeViewItemParent(node) as TreeViewItem;
                if (parentNodeControl != null && parentNodeControl.Header != null)
                {
                    node = parentNodeControl as TreeViewItem;
                }
            }
            string[] separators = { ":" };
            foreach (var item in node.Items)
            {
                TreeViewItem child = item as TreeViewItem;
                string nodeText = child.Header.ToString();

                if (nodeText.StartsWith("DeviceID"))
                {
                    deviceId = nodeText.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else if (nodeText.StartsWith("Caption"))
                {
                    description = nodeText.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else if (nodeText.StartsWith("PNPDeviceID"))
                {
                    pnpDeviceId = nodeText.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                else if (nodeText.StartsWith("Description") && description == "")
                {
                    description = nodeText.Split(separators, StringSplitOptions.RemoveEmptyEntries)[1];
                }
            }

            if (deviceId.StartsWith(" USB")) 
            {
                deviceId = deviceId.Remove(0, 5);
            } 
            newDevice.VID = deviceId;
            newDevice.PID = deviceId;
            newDevice.Name = description;
            newDevice.PnPID = pnpDeviceId;
            currentSelectedDevice = newDevice;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow()
            {
                Owner = this,
                ShowInTaskbar = false,
            };
            settingsWindow.ShowDialog();
        }

        #endregion Events

        #region Worker

        private void worker1_DoWork(object sender, DoWorkEventArgs e)
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PointingDevice'" +
                "OR TargetInstance ISA 'Win32_Keyboard' OR TargetInstance ISA 'Win32_DiskDrive'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_PointingDevice'" +
                "OR TargetInstance ISA 'Win32_Keyboard' OR TargetInstance ISA 'Win32_DiskDrive'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();

            // Do something while waiting for events
            System.Threading.Thread.Sleep(20000000);
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            List<string> props = new List<string>();
            string descriptor = "";
            foreach (var property in instance.Properties)
            {
                if (property.Name == "Description")
                {
                    descriptor = (string)property.Value;
                }
                props.Add(property.Name + ": " + property.Value);
            }
            UpdateTree(EventType.Insert, descriptor, props);
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            List<string> props = new List<string>();
            string descriptor = "";
            foreach (var property in instance.Properties)
            {
                if (property.Name == "Description")
                {
                    descriptor = (string)property.Value;
                }
                props.Add(property.Name + ": " + property.Value);
            }
            UpdateTree(EventType.Remove, descriptor, props);
        }




        #endregion Worker


    }
}
