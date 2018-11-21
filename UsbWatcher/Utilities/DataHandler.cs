using System;
using System.Data.SQLite;
using System.IO;
using System.Net;

namespace UsbWatcher
{
    class DataHandler
    {
        private static readonly string[] separator = { "  " }; // Separator between ID number and name
        private static readonly string dbString = @"Data Source=" + Constants.dbDefaultLocation;

        public static void InitializeDatabase()
        {
            using (SQLiteConnection db = new SQLiteConnection(dbString))
            {
                db.Open();

                string[] tableCommands = {
                    "CREATE TABLE IF NOT EXISTS VendorIds (Vendor_Id VARCHAR(10), Vendor_Name VARCHAR(50), PRIMARY KEY(Vendor_Id))",
                    "CREATE TABLE IF NOT EXISTS ProductIds (Product_Id VARCHAR(10), Vendor_Id VARCHAR(10), Product_Name VARCHAR(50), PRIMARY KEY(Product_Id, Vendor_Id))",
                    "CREATE TABLE IF NOT EXISTS local_VendorIds (Vendor_Id VARCHAR(10), Vendor_Name VARCHAR(50), PRIMARY KEY(Vendor_Id))",
                    "CREATE TABLE IF NOT EXISTS local_ProductIds (Product_Id VARCHAR(10), Vendor_Id VARCHAR(10), Product_Name VARCHAR(50), PRIMARY KEY(Product_Id, Vendor_Id))",
                    "CREATE TABLE IF NOT EXISTS local_MetaData (id INTEGER PRIMARY KEY, Attribute VARCHAR(25), Value TEXT)"
                };

                foreach (var command in tableCommands)
                {
                    SQLiteCommand createTable = new SQLiteCommand(command, db);
                    createTable.ExecuteReader();
                }

                db.Close();
            }
        }

        private static int FileLength()
        {
            int lineCount = 0;
            string line = "";

            using (StreamReader sr = new StreamReader(IdReader.usbIdList))
            {
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
                    else
                    {
                        lineCount++;
                    }
                }
            }
            return lineCount;
        }

        public static void FileToDb()
        {
            string filename = Constants.usbIdFileDefaultLocation;

            if (!File.Exists(Constants.dbDefaultLocation))
            {
                InitializeDatabase();
            }

            if (!File.Exists(filename))
            {
                DownloadIdFile();
            }

            var progressWindow = new SetupWindow();
            progressWindow.SetProgressBarMax(FileLength());
            progressWindow.Show();

            using (StreamReader sr = new StreamReader(filename))
            using (SQLiteConnection db = new SQLiteConnection(dbString))
            {
                db.Open();

                int processed = 0;
                string line;
                string curVendorId = "";
                string curVendorName = "";

                using (var cmd = new SQLiteCommand(db))
                using (var transaction = db.BeginTransaction())
                {
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
                        else
                        {
                            if (!string.IsNullOrEmpty(line) && (line[0] != '\t' && line[1] != '\t'))
                            {
                                string[] vendorData = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                                curVendorId = vendorData[0];
                                curVendorName = vendorData[1];

                                cmd.CommandText = "INSERT INTO VendorIds VALUES (@id, @name)";
                                cmd.Parameters.AddWithValue("@id", curVendorId);
                                cmd.Parameters.AddWithValue("@name", curVendorName);
                                cmd.ExecuteNonQuery();
                            }
                            else if (!string.IsNullOrEmpty(line) && (line[0] == '\t' && line[1] != '\t'))
                            {
                                string[] prodData = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                                var prodId = prodData[0];
                                var prodName = prodData[1];

                                prodId = prodId.TrimStart('\t');

                                cmd.CommandText = "INSERT INTO ProductIds VALUES (@pid, @vid, @name)";
                                cmd.Parameters.AddWithValue("@pid", prodId);
                                cmd.Parameters.AddWithValue("@vid", curVendorId);
                                cmd.Parameters.AddWithValue("@name", prodName);
                                cmd.ExecuteNonQuery();
                            }

                            processed++;
                            progressWindow.UpdateProgressBar(processed);
                        }
                    }
                    transaction.Commit();
                }
                db.Close();
                progressWindow.Close();
            }
        }

        public static string GetVendor(string vendorId)
        {
            string vendorName = "";
            using (SQLiteConnection db = new SQLiteConnection(dbString))
            {
                db.Open();
                try
                {
                    var cmd = new SQLiteCommand
                    {
                        CommandText = "SELECT [Vendor_Name] FROM [VendorIds] WHERE [Vendor_Id] = @id",
                        Connection = db
                    };
                    cmd.Parameters.AddWithValue("@id", vendorId.ToLower());

                    vendorName = cmd.ExecuteScalar().ToString();
                }
                catch (Exception)
                {
                    vendorName = "Name not found.";
                }

                db.Close();
            }
            return vendorName;
        }

        public static string GetProduct(string productId, string vendorId)
        {
            string productName = "";
            using (SQLiteConnection db = new SQLiteConnection(dbString))
            {
                db.Open();
                try
                {
                    var cmd = new SQLiteCommand
                    {
                        CommandText = "SELECT [Product_Name] FROM [ProductIds] WHERE [Vendor_Id] = '@vid' AND [Product_Id] = @pid",
                        Connection = db
                    };
                    cmd.Parameters.AddWithValue("@vid", vendorId.ToLower());
                    cmd.Parameters.AddWithValue("@pid", productId.ToLower());

                    productName = cmd.ExecuteScalar().ToString();
                }
                catch (Exception)
                {
                    productName = "Name not found.";
                }
            }
            return productName;
        }

        public static void DownloadIdFile()
        {
            // Create download folder?

            using (var w = new WebClient())
            {
                w.DownloadFile(new Uri(Constants.usbIdFileUrl), Constants.usbIdFileDefaultLocation);
            }
            using (SQLiteConnection db = new SQLiteConnection(dbString))
            {
                db.Open();

                var cmd = new SQLiteCommand
                {
                    CommandText = "REPLACE INTO [local_MetaData] (Attribute, Value) VALUES ('Last_Download', @date)",
                    Connection = db
                };
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("d"));
                cmd.ExecuteNonQuery();

                db.Close();
            }
        }

        public static void UpdateDatabase()
        {
            using (SQLiteConnection db = new SQLiteConnection(dbString))
            {
                db.Open();
                try
                {
                    var cmd = new SQLiteCommand
                    {
                        CommandText = "REPLACE INTO [local_MetaData] (Attribute, Value) VALUES ('Last_Update', @date)",
                        Connection = db
                    };
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("d"));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {

                }
                db.Close();
            }
        }

        public static string GetLast(bool download = false)
        {
            string last = "";

            string attr;
            if (download)
            {
                attr = "Last_Download";
            }
            else
            {
                attr = "Last_Update";
            }

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(dbString))
                {
                    db.Open();
                    var cmd = new SQLiteCommand
                    {
                        CommandText = "SELECT [Value] FROM [local_MetaData] WHERE [Attribute] = @attr",
                        Connection = db
                    };
                    cmd.Parameters.AddWithValue("@attr", attr);

                    last = cmd.ExecuteScalar().ToString();

                    db.Close();
                }
            }
            catch (Exception)
            {
                last = "UNKNOWN";
            }

            

            return last;
        }
    }
}
