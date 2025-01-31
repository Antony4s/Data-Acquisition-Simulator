using DataAcquisitionSimulatorNew.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace DataAcquisitionSimulatorNew.Services
{
    public sealed class SensorSettingsManager
    {
        private static readonly SensorSettingsManager _instance = new SensorSettingsManager();
        private const string SettingsFilePath = "sensorSettings.json";

        public ObservableCollection<Sensor> Sensors { get; private set; }

        // Private constructor to prevent external instantiation
        private SensorSettingsManager()
        {
            Sensors = LoadSettings() ?? InitializeDefaultSensors();
        }

        public static SensorSettingsManager Instance => _instance;

        // Save settings to a file
        public void SaveSettings()
        {
            //string json = JsonSerializer.Serialize(Sensors, new JsonSerializerOptions { WriteIndented = true });
            //File.WriteAllText(SettingsFilePath, json);
        }

        // Load settings from a file
        public ObservableCollection<Sensor> LoadSettings()
        {
            if (!File.Exists(SettingsFilePath)) return null;

            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<ObservableCollection<Sensor>>(json);
        }

        // Initialize default sensors if no settings are found
        public ObservableCollection<Sensor> InitializeDefaultSensors()
        {
            return new ObservableCollection<Sensor>
            {
                new Sensor("Temperature", -10, 50) { Threshold = 40, TrendStep = 0.5, NoiseLevel = 1.0 },
                new Sensor("Humidity", 0, 100) { Threshold = 90, TrendStep = 1.0, NoiseLevel = 2.0 },
                new Sensor("Pressure", 900, 1100) { Threshold = 1050, TrendStep = 2.0, NoiseLevel = 1.0 }
            };
        }
    }
}
