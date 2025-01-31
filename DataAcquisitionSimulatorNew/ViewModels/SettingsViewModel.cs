using DataAcquisitionSimulatorNew.Models;
using DataAcquisitionSimulatorNew.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcquisitionSimulatorNew.ViewModels
{
    public class SettingsViewModel
    {
        private readonly SensorSettingsManager _settingsManager;

        public ObservableCollection<Sensor> Sensors => _settingsManager.Sensors;

        public SettingsViewModel()
        {
            _settingsManager = SensorSettingsManager.Instance;


        }
    }
}
