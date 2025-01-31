using DataAcquisitionSimulatorNew.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;


namespace DataAcquisitionSimulatorNew.Services
{
    public class SensorSettingsService
    {
        private static SensorSettingsService _instance;
        private const string SettingsFilePath = "SensorSettings.json"; // File to store serialized settings

        public static SensorSettingsService Instance => _instance ??= new SensorSettingsService();

        public SensorSettingsService() { }

        // Save settings to file
        public void SaveSettings(IEnumerable<Sensor> sensors)
        {
            string json = JsonSerializer.Serialize(sensors, new JsonSerializerOptions
            {
                WriteIndented = true // Makes the JSON more readable
            });
            File.WriteAllText(SettingsFilePath, json);
        }

        // Load settings from file
        public IEnumerable<Sensor> LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<List<Sensor>>(json);
            }
            return new List<Sensor>(); // Return empty list if no file exists
        }
    }
}
