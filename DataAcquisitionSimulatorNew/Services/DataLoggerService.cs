using DataAcquisitionSimulatorNew.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;

namespace DataAcquisitionSimulatorNew.Services
{
    public class DataLoggerService
    {
        private Timer _timer;
        private ObservableCollection<Sensor> _sensors;
        private string _logFilePath;
        private string _lastLoggedLine; // Store the last logged line

        public DataLoggerService(ObservableCollection<Sensor> sensors)
        {
            _sensors = sensors;
        }

        public void StartLogging(string filePath, int intervalMilliseconds = 1000, string simulationMode = "Random")
        {
            _logFilePath = filePath;

            using (StreamWriter writer = new StreamWriter(_logFilePath, false))
            {
                writer.WriteLine($"Simulation Mode: {simulationMode}");
                writer.WriteLine("Timestamp,Temperature (°C),Humidity (%),Pressure (hPa)");
            }

            _timer = new Timer(intervalMilliseconds);
            _timer.Elapsed += LogData;
            _timer.Start();
        }



        public void StopLogging()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }
        }



        private void LogData(object sender, ElapsedEventArgs e)
        {
            string timestamp = DateTime.Now.ToString("o"); // ISO 8601 format
            List<string> dataLine = new List<string> { timestamp }; // Start with the timestamp

            foreach (Sensor sensor in _sensors)
            {
                // Use InvariantCulture to ensure consistent decimal formatting
                string formattedValue = sensor.Values.LastOrDefault().ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                dataLine.Add(formattedValue); // Add the formatted value for each sensor
            }

            // Join values with a comma and write to the CSV file
            string csvLine = string.Join(",", dataLine);

            if (csvLine != _lastLoggedLine) // Check for duplicate data
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true, new System.Text.UTF8Encoding(true)))
                {
                    writer.WriteLine(csvLine);
                }
                _lastLoggedLine = csvLine; // Update the last logged line
            }
        }

        public void PauseLogging()
        {
            _timer?.Stop();
        }

        public void ResumeLogging()
        {
            _timer?.Start();
        }



    }
}