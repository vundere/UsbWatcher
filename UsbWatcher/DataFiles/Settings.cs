using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config.Net;

namespace UsbWatcher
{
    public interface IUserSettings
    {
        [Option(DefaultValue = 5)]
        int DownloadMaxAttempts { get; set; }

        [Option(DefaultValue = 0.5)]
        double DownloadAttemptCooldown { get; set; }

        [Option(DefaultValue = 2.0)]
        double DownloadAttemptExponential { get; set; }

        [Option(DefaultValue = false)]
        bool DbExists { get; set; }

        [Option(DefaultValue = false)]
        bool IdFileExists { get; set; }

    }
}
