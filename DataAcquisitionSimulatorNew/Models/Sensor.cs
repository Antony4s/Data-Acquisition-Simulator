using DataAcquisitionSimulatorNew.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
namespace DataAcquisitionSimulatorNew.Models
{
    public class Sensor
    {

        public string Name { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double Threshold { get; set; }
        public ObservableCollection<double> Values { get; set; }
        public ObservableCollection<string> Timestamps { get; set; } // New property for timestamps
        private readonly FakeSensorDataGenerator _dataGenerator;
        public double CurrentValue { get; set; }
        public double TrendStep { get; set; } = 0.5; // Default trend step
        public double NoiseLevel { get; set; } = 1.0; // Default noise level
        public FakeSensorDataGenerator.SimulationMode SimulationMode
        {
            get => _dataGenerator.CurrentMode;
            set
            {
                _dataGenerator.CurrentMode = value; // Ensure this updates the generator
            }
        }

        public bool IsThresholdBreached()
        {
            return CurrentValue > Threshold;
        }

        public Sensor(string name, double minValue, double maxValue)
        {
            Name = name;
            MinValue = minValue;
            MaxValue = maxValue;
            Values = new ObservableCollection<double>();
            Timestamps = new ObservableCollection<string>();
            _dataGenerator = new FakeSensorDataGenerator(); // Initialize the generato
        }

        public void UpdateValues()
        {
            double newValue = 0;

            // Generate value based on the selected mode
            switch (SimulationMode)
            {
                case FakeSensorDataGenerator.SimulationMode.Linear:
                    newValue = _dataGenerator.GenerateLinearValue(CurrentValue, MinValue, MaxValue, TrendStep);
                    break;

                case FakeSensorDataGenerator.SimulationMode.Sinusoidal:
                    newValue = _dataGenerator.GenerateSinusoidalValue(MinValue, MaxValue);
                    break;

                case FakeSensorDataGenerator.SimulationMode.RandomSpikes:
                    newValue = _dataGenerator.GenerateRandomSpikeValue(CurrentValue, MinValue, MaxValue);
                    break;

                case FakeSensorDataGenerator.SimulationMode.Flatline:
                    newValue = CurrentValue; // Keep the value constant
                    break;
            }

            // Apply noise and clamp within range
            newValue = _dataGenerator.GenerateValueWithNoise(newValue, NoiseLevel);
            newValue = Math.Clamp(newValue, MinValue, MaxValue);

            // Add the new value to the list
            Values.Add(newValue);
            Timestamps.Add(DateTime.Now.ToString("o")); // Add timestamp in ISO format

            // Limit the size of the Values list (e.g., last 50 values)
            if (Values.Count > 50)
            {
                Values.RemoveAt(0);
                Timestamps.RemoveAt(0);
            }

            // Update the current value for continuity
            CurrentValue = newValue;

            // Notify MainViewModel to update Y-axis range
            OnYAxisUpdate?.Invoke();
        }
        // Add a delegate to notify the ViewModel
        public Action OnYAxisUpdate { get; set; }

        public void UpdateCurrentValue(double newValue)
        {
            CurrentValue = newValue; // Update the current value
            Values.Add(newValue);    // Add the value to the chart data

            // Limit the number of points to prevent memory issues
            if (Values.Count > 50)
            {
                Values.RemoveAt(0); // Remove the oldest value
            }
        }


    }
}
