using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcquisitionSimulatorNew.Helpers
{
    public static class TimeFormatter
    {
        public static string FormatTime(DateTime timestamp)
        {
            return timestamp.ToString("HH:mm:ss");
        }
    }
}
