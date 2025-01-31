using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcquisitionSimulatorNew.Services
{
    public static class FileDialogService
    {
        public static string GetSaveFilePath(string filter)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = filter
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public static string GetOpenFilePath(string filter)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = filter
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
