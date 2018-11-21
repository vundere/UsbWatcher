using System;
using System.IO;

namespace UsbWatcher
{
    class IdReader
    {

        #region Fields

        public static readonly string usbIdList = @"W:\GitHub\csharp\UsbWatcher\UsbWatcher\DataFiles\usb.ids";

        public static readonly char[] separators = { '&', '_'}; // Separators for the full Device ID string

        private static readonly string[] separator = { "  " }; // Separator between ID number and name

        #endregion Fields

        #region Static Methods

        public static string ExtractVendorId(string deviceId)
        {
            string vendorId = "";
            string[] substrings = deviceId.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string substring in substrings)
            {
                if (substring == "VID")
                {
                    vendorId = substrings[Array.IndexOf(substrings, substring) + 1]; //The ID should be the next value in the array.
                }
            }
            return vendorId;
        }

        public static string ExtractProductId(string deviceId)
        {
            string productId = "";

            string[] substrings = deviceId.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string substring in substrings)
            {
                if (substring == "PID")
                {
                    productId = substrings[Array.IndexOf(substrings, substring) + 1]; //The ID should be the next value in the array.
                }
            }
            return productId;
        }


        // Obsolete string-search solution
        public static string LookupVendor(string vendorId)
        {
            string vendor = "";

            try
            {
                using (StreamReader sr = new StreamReader(usbIdList))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith(vendorId.ToLower()))
                        {
                            vendor = line.Split(new[] { "  " }, StringSplitOptions.RemoveEmptyEntries)[1];
                            break;
                        }
                    }

                }
            }
            catch
            {
                vendor = "Could not find vendor.";
            }

            return vendor;
        }

        public static string LookupProduct(string vendorId, string productId)
        {
            string product = "";

            try
            {
                using (StreamReader sr = new StreamReader(usbIdList))
                {
                    string line;
                    string curVendorId = "";
                    string curVendorName = "";
                    bool vendorIdFound = false;



                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("#"))
                        {
                            continue;
                        }
                        else if (line == "")
                        {
                            continue;
                        }
                        else if (!string.IsNullOrEmpty(line) && (line[0] != '\t' && line[1] != '\t'))
                        {

                            if (vendorIdFound)
                            {
                                throw new Exception();
                            }

                            string[] vendorData = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            curVendorId = vendorData[0];
                            curVendorName = vendorData[1];

                            if (curVendorId == vendorId)
                            {
                                vendorIdFound = true;
                            }

                        }
                        else if (!string.IsNullOrEmpty(line) && (line[0] == '\t' && line[1] != '\t'))
                        {
                            string[] prodData = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            var prodId = prodData[0];
                            var prodName = prodData[1];

                            if (prodId == productId && curVendorId == vendorId)
                            {
                                return prodName;
                            }

                        }
                    }

                }
            }
            catch
            {
                product = "Could not find product.";
            }

            return product;
        }


        #endregion Static Methods

    }
}
