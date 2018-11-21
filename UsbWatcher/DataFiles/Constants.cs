using System.IO;
using System.Reflection;

namespace UsbWatcher
{
    class Constants
    {
        public static readonly string usbIdFileUrl = @"http://www.linux-usb.org/usb.ids";

        public static readonly string usbIdFileDefaultLocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\usb.ids";

        public static readonly string dbDefaultLocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\usbIds.db";

        public static readonly string SettingsPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\Settings.ini";
    }
}
