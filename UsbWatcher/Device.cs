using System;

namespace UsbWatcher
{
    public class Device
    {
        private string _VID;
        private string _PID;
        private string _PnPID;

        public String Name { get; set; }
        public String VID
        {
            get { return this._VID; }
            set { _VID = IdReader.ExtractVendorId(value); }
        }
        public String PID
        {
            get { return this._PID; }
            set { _PID = IdReader.ExtractProductId(value); }
        }
        public String PnPID
        {
            get { return this._PnPID; }
            set { this._PnPID = value; }
        }
    }
}
