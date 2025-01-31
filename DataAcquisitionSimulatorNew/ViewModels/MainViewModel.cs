using AcquisitionShared.Protocol;
using DataAcquisitionSimulatorNew.Helpers;
using DataAcquisitionSimulatorNew.Models;
using DataAcquisitionSimulatorNew.Services;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static DataAcquisitionSimulatorNew.Helpers.FakeSensorDataGenerator;
namespace DataAcquisitionSimulatorNew.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        // TCP client and stream
        private TcpClient _client;
        private NetworkStream _stream;
        private class SensorData
        {
            [JsonPropertyName("Temperature")]
            public double Temperature { get; set; }

            [JsonPropertyName("Humidity")]
            public double Humidity { get; set; }

            [JsonPropertyName("Pressure")]
            public double Pressure { get; set; }
        }

        private readonly SensorSettingsManager _settingsManager;
        public ObservableCollection<Sensor> Sensors => _settingsManager.Sensors;
        public ObservableCollection<ISeries> Series { get; set; }
        public ObservableCollection<Axis> YAxes { get; set; }

        private readonly DataLoggerService _dataLoggerService;

        public ICommand ConnectCommand => new RelayCommand(async () => await ConnectToAcquisitionServer());
        public ICommand StartLoggingCommand { get; }
        public ICommand StopLoggingCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand PauseLoggingCommand { get; }
        public ICommand ResumeLoggingCommand { get; }
        public ICommand StartAcquisitionCommand { get; }
        public ICommand StopAcquisitionCommand { get; }
        public ICommand ChangeThemeCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }

        private bool _isSettingsVisible = true;
        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set
            {
                _isSettingsVisible = value;
                OnPropertyChanged(nameof(IsSettingsVisible));
            }
        }

        public ICommand ToggleSettingsCommand { get; }


        private void ToggleSettings()
        {
            IsSettingsVisible = !IsSettingsVisible;
        }
        //private readonly SensorSettingsService _sensorSettingsService = new();



        private bool _isLoggingActive;
        public bool IsLoggingActive
        {
            get => _isLoggingActive;
            set
            {
                _isLoggingActive = value;
                OnPropertyChanged(nameof(IsLoggingActive));
                OnPropertyChanged(nameof(IsLoggingStopped)); // Notify dependent property
            }
        }

        public bool IsLoggingStopped => !_isLoggingActive;

        public event PropertyChangedEventHandler PropertyChanged;
        private double _temperatureMin = 0;
        public double TemperatureMin
        {
            get => _temperatureMin;
            set
            {
                _temperatureMin = value;
                OnPropertyChanged(nameof(TemperatureMin));
                UpdateSensorRanges();
            }
        }

        private double _temperatureMax = 50;
        public double TemperatureMax
        {
            get => _temperatureMax;
            set
            {
                _temperatureMax = value;
                OnPropertyChanged(nameof(TemperatureMax));
                UpdateSensorRanges();
            }
        }
        //Humidity
        private double _humidityTrendStep = 1.0;
        public double HumidityTrendStep
        {
            get => _humidityTrendStep;
            set
            {
                _humidityTrendStep = value;
                OnPropertyChanged(nameof(_humidityTrendStep));
                UpdateSensorTrend("Humidity");
            }
        }

        private double _humidityNoiseLevel = 2.0;
        public double HumidityNoiseLevel
        {
            get => _humidityNoiseLevel;
            set
            {
                _humidityNoiseLevel = value;
                OnPropertyChanged(nameof(_humidityNoiseLevel));
                UpdateSensorNoise("Humidity");
            }
        }
        //Temperature
        private double _temperatureTrendStep = 0.5;
        public double TemperatureTrendStep
        {
            get => _temperatureTrendStep;
            set
            {
                _temperatureTrendStep = value;
                OnPropertyChanged(nameof(TemperatureTrendStep));
                UpdateSensorTrend("Temperature");
            }
        }

        private double _temperatureNoiseLevel = 1.0;
        public double TemperatureNoiseLevel
        {
            get => _temperatureNoiseLevel;
            set
            {
                _temperatureNoiseLevel = value;
                OnPropertyChanged(nameof(TemperatureNoiseLevel));
                UpdateSensorNoise("Temperature");
            }
        }
        //Pressure
        private double _pressureTrendStep = 2.0;
        public double PressureTrendStep
        {
            get => _pressureTrendStep;
            set
            {
                _pressureTrendStep = value;
                OnPropertyChanged(nameof(_pressureTrendStep));
                UpdateSensorTrend("Pressure");
            }
        }

        private double _pressureNoiseLevel = 1.0;
        public double PressureNoiseLevel
        {
            get => _pressureNoiseLevel;
            set
            {
                _pressureNoiseLevel = value;
                OnPropertyChanged(nameof(_pressureNoiseLevel));
                UpdateSensorNoise("Pressure");
            }
        }
        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        }
        private int _loggingInterval = 1000; // Default 1 second
        public int LoggingInterval
        {
            get => _loggingInterval;
            set
            {
                _loggingInterval = value;
                OnPropertyChanged(nameof(LoggingInterval));
            }
        }
        // Add this property to expose the modes to the UI
        public Array SimulationModes => Enum.GetValues(typeof(SimulationMode));

        // Add a dictionary to map sensors to their selected modes
        public Dictionary<string, SimulationMode> SensorModes { get; set; }

        public double YAxisMin { get; set; } = 0;
        public double YAxisMax { get; set; } = 100;

        public ObservableCollection<string> AvailableSensors { get; set; }
        private string _selectedSensor;
        public string SelectedSensor
        {
            get => _selectedSensor;
            set
            {
                _selectedSensor = value;
                OnPropertyChanged(nameof(SelectedSensor));
                UpdateSelectedSensor();
            }
        }

        // Properties for statistics
        private double _average;
        public double Average
        {
            get => _average;
            set
            {
                _average = value;
                OnPropertyChanged(nameof(Average));
            }
        }

        private double _minValue;
        public double MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                OnPropertyChanged(nameof(MinValue));
            }
        }

        private double _maxValue;
        public double MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                OnPropertyChanged(nameof(MaxValue));
            }
        }

        private double _standardDeviation;
        public double StandardDeviation
        {
            get => _standardDeviation;
            set
            {
                _standardDeviation = value;
                OnPropertyChanged(nameof(StandardDeviation));
            }
        }

        private string _loggingStatusMessage = "Logging Stopped";
        public string LoggingStatusMessage
        {
            get => _loggingStatusMessage;
            set
            {
                _loggingStatusMessage = value;
                OnPropertyChanged(nameof(LoggingStatusMessage));
            }
        }

        private SolidColorBrush _loggingStatusColor = new SolidColorBrush(Colors.Red);
        public SolidColorBrush LoggingStatusColor
        {
            get => _loggingStatusColor;
            set
            {
                _loggingStatusColor = value;
                OnPropertyChanged(nameof(LoggingStatusColor));
            }
        }

        private Visibility _loggingStatusVisibility = Visibility.Visible;
        public Visibility LoggingStatusVisibility
        {
            get => _loggingStatusVisibility;
            set
            {
                _loggingStatusVisibility = value;
                OnPropertyChanged(nameof(LoggingStatusVisibility));
            }
        }

        // Timer for hiding the notification after a delay
        private readonly DispatcherTimer _notificationTimer;

        private string _alertMessage;
        public string AlertMessage
        {
            get => _alertMessage;
            set
            {
                _alertMessage = value;
                OnPropertyChanged(nameof(AlertMessage));
            }
        }

        private readonly DispatcherTimer _alertTimer;
        public MainViewModel()
        {
            _notificationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _notificationTimer.Tick += (s, e) =>
            {
                LoggingStatusVisibility = Visibility.Collapsed;
                _notificationTimer.Stop();
            };

            // Initialize the timer for alerts
            _alertTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _alertTimer.Tick += (s, e) =>
            {
                AlertMessage = string.Empty; // Clear the alert message
                _alertTimer.Stop();
            };
            _settingsManager = SensorSettingsManager.Instance;

            // Default visibility
            LoggingStatusVisibility = Visibility.Collapsed;
            AvailableSensors = new ObservableCollection<string>
            {
                "Temperature",
                "Humidity",
                "Pressure"
            };

            StartAcquisitionCommand = new RelayCommand(StartAcquisition, CanSendCommand);
            StopAcquisitionCommand = new RelayCommand(StopAcquisition, CanSendCommand);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
            LoadSettingsCommand = new RelayCommand(LoadSettings);
            ToggleSettingsCommand = new RelayCommand(ToggleSettings);


            // Initialize sensors
            //Sensors = new ObservableCollection<Sensor>
            //{
            //    new Sensor("Temperature", -10, 50) { Threshold = 40, TrendStep = 0.5, NoiseLevel = 1.0 },
            //    new Sensor("Humidity", 0, 100) { Threshold = 90, TrendStep = 1.0, NoiseLevel = 2.0 },
            //    new Sensor("Pressure", 900, 1100) { Threshold = 1050, TrendStep = 2.0, NoiseLevel = 1.0 }
            //};


            // Initialize the dictionary with default values
            SensorModes = new Dictionary<string, SimulationMode>
            {
                { "Temperature", SimulationMode.Linear },
                { "Humidity", SimulationMode.Sinusoidal },
                { "Pressure", SimulationMode.RandomSpikes }
            };

            // Update sensors to use the default modes
            foreach (Sensor sensor in Sensors)
            {
                sensor.SimulationMode = SensorModes[sensor.Name];
            }

            foreach (Sensor sensor in Sensors)
            {
                sensor.OnYAxisUpdate = UpdateYAxisRange; // No overload issues now
            }

            // Initialize series dynamically
            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Values = Sensors.First(s => s.Name == "Temperature").Values,
                    Name = $"Temperature ({SensorModes["Temperature"]})",
                    Fill = new SolidColorPaint(SKColors.LightSkyBlue.WithAlpha(60)),
                    LineSmoothness = 0.8,
                    Stroke = new SolidColorPaint(SKColors.DodgerBlue)
                    {
                        StrokeThickness = 3
                    },
                    GeometrySize = 10,
                    EasingFunction = EasingFunctions.CubicInOut // Customize easing for animation
                },
                //new LineSeries<double>
                //{
                //    Values = Sensors.First(s => s.Name == "Humidity").Values,
                //    Name = $"Humidity ({SensorModes["Humidity"]})",
                //    GeometrySize = 10
                //},
                //new LineSeries<double>
                //{
                //    Values = Sensors.First(s => s.Name == "Pressure").Values,
                //    Name = $"Pressure ({SensorModes["Pressure"]})",
                //    GeometrySize = 10
                //}
            };

            YAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "Values",
                    MinLimit = YAxisMin,
                    MaxLimit = YAxisMax,
                    NamePaint = new SolidColorPaint(SKColors.Black)
                }
            };

            SelectedSensor = "Temperature"; // Default selection

            // Update statistics whenever sensor data changes
            foreach (Sensor sensor in Sensors)
            {
                sensor.Values.CollectionChanged += (s, e) => UpdateStatistics();
            }

            // Set initial statistics for the selected sensor
            UpdateStatistics();

            // Initialize the data logger service
            _dataLoggerService = new DataLoggerService(Sensors);

            // Initialize commands
            StartLoggingCommand = new RelayCommand(StartLogging);
            StopLoggingCommand = new RelayCommand(StopLogging);

            UpdateCommand = new RelayCommand(UpdateSensorValues);
            ExportCommand = new RelayCommand(ExportData);
            ImportCommand = new RelayCommand(ImportData);
            PauseLoggingCommand = new RelayCommand(PauseLogging, () => IsLoggingActive && !IsPaused);
            ResumeLoggingCommand = new RelayCommand(ResumeLogging, () => IsLoggingActive && IsPaused);


            // Default logging state
            IsLoggingActive = false;
        }

        private void LoadSettings()
        {
            //List<Sensor> loadedSensors = _sensorSettingsService.LoadSettings();
            //if (loadedSensors.Count > 0)
            //{
            //    Sensors.Clear();
            //    foreach (Sensor sensor in loadedSensors)
            //    {
            //        Sensors.Add(sensor);
            //    }

            //    UpdateSelectedSensor();
            //    ShowNotification("Settings loaded successfully!", new SolidColorBrush(Colors.Green));
            //}
            //else
            //{
            //    ShowNotification("No settings found to load.", new SolidColorBrush(Colors.Red));
            //}
        }

        private void SaveSettings()
        {
            //_sensorSettingsService.SaveSettings(Sensors);
            ShowNotification("Settings saved successfully!", new SolidColorBrush(Colors.Green));
        }

        private void StartLogging()
        {
            string filePath = FileDialogService.GetSaveFilePath("CSV Files (*.csv)|*.csv");
            if (!string.IsNullOrEmpty(filePath))
            {
                _dataLoggerService.StartLogging(filePath, LoggingInterval);
                IsLoggingActive = true; // Logging is now active
                IsPaused = false; // Reset pause state

                ShowNotification("Logging Started", new SolidColorBrush(Colors.Green));
            }
        }

        private void StopLogging()
        {
            _dataLoggerService.StopLogging();
            IsLoggingActive = false; // Logging is no longer active
            IsPaused = false; // Reset pause state
            ShowNotification("Logging Stopped", new SolidColorBrush(Colors.Red));
        }

        public void UpdateSensorValues()
        {
            // Find the currently selected sensor
            Sensor? selectedSensor = Sensors.FirstOrDefault(s => s.Name == SelectedSensor);

            // Update only the selected sensor
            if (selectedSensor != null)
            {
                // Generate a new value and add it to the sensor's data
                selectedSensor.UpdateValues();

                // Optionally limit the number of data points for better performance
                if (selectedSensor.Values.Count > 100) // Keep the latest 100 points
                {
                    selectedSensor.Values.RemoveAt(0);
                }

                // Update the chart's Y-axis to match the new data range
                UpdateYAxisRange();
            }

            // Check thresholds for the selected sensor
            CheckThresholds();
        }


        private void ExportData()
        {
            string filePath = FileDialogService.GetSaveFilePath("Excel Files (*.xlsx)|*.xlsx");
            if (filePath != null)
            {
                // Convert ObservableCollection<Sensor> to List<Sensor>
                List<Sensor> sensorList = Sensors.ToList();
                ExcelService.ExportToExcel(filePath, sensorList);
            }
        }

        private void ImportData()
        {
            string filePath = FileDialogService.GetOpenFilePath("Excel Files (*.xlsx)|*.xlsx");
            if (filePath != null)
            {
                System.Collections.Generic.List<System.Collections.Generic.List<double>> importedData = ExcelService.ImportFromExcel(filePath);
                for (int i = 0; i < Sensors.Count; i++)
                {
                    Sensors[i].Values.Clear();
                    foreach (double value in importedData[i])
                    {
                        Sensors[i].Values.Add(value);
                    }
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void UpdateSensorRanges()
        {
            Sensor? temperatureSensor = Sensors.FirstOrDefault(s => s.Name == "Temperature");
            if (temperatureSensor != null)
            {
                temperatureSensor.MinValue = TemperatureMin;
                temperatureSensor.MaxValue = TemperatureMax;
            }
            // Repeat for Humidity and Pressure sensors
        }

        private void UpdateSensorTrend(string sensorName)
        {
            Sensor? sensor = Sensors.FirstOrDefault(s => s.Name == sensorName);
            if (sensor != null)
            {
                switch (sensorName)
                {
                    case "Temperature":
                        sensor.TrendStep = TemperatureTrendStep; break;
                    case "Humidity":
                        sensor.TrendStep = HumidityTrendStep; break;
                    case "Pressure":
                        sensor.TrendStep = PressureTrendStep; break;
                }


            }
            else
            {
                Debug.WriteLine($"Sensor '{sensorName}' not found.");
            }
        }

        private void UpdateSensorNoise(string sensorName)
        {
            Sensor? sensor = Sensors.FirstOrDefault(s => s.Name == sensorName);
            if (sensor != null)
            {
                switch (sensorName)
                {
                    case "Temperature":
                        sensor.NoiseLevel = TemperatureNoiseLevel; break;
                    case "Humidity":
                        sensor.NoiseLevel = HumidityNoiseLevel; break;
                    case "Pressure":
                        sensor.NoiseLevel = PressureNoiseLevel; break;
                }


            }
            else
            {
                Debug.WriteLine($"Sensor '{sensorName}' not found.");
            }
        }

        private void PauseLogging()
        {
            _dataLoggerService.PauseLogging();
            IsPaused = true; // Update the state

            CommandManager.InvalidateRequerySuggested(); // Refresh commands
        }

        private void ResumeLogging()
        {
            _dataLoggerService.ResumeLogging();
            IsPaused = false; // Update the state

            CommandManager.InvalidateRequerySuggested(); // Refresh commands
        }
        public void UpdateSensorMode(string sensorName, FakeSensorDataGenerator.SimulationMode mode)
        {
            Sensor? sensor = Sensors.FirstOrDefault(s => s.Name == sensorName);
            if (sensor != null)
            {
                sensor.SimulationMode = mode; // Update the mode
                SensorModes[sensorName] = mode; // Update the dictionary
                OnPropertyChanged(nameof(SensorModes)); // Notify the UI
            }
        }

        private void UpdateSelectedSensor()
        {
            Series.Clear();

            Sensor? selectedSensor = Sensors.FirstOrDefault(s => s.Name == SelectedSensor);
            if (selectedSensor != null)
            {
                Series.Add(new LineSeries<double>
                {
                    Values = selectedSensor.Values,
                    Name = $"{selectedSensor.Name} ({GetUnitForSensor(selectedSensor.Name)})",
                    GeometrySize = 10
                });

                // Update statistics for the selected sensor
                UpdateStatistics();

                // Update Y-axis
                UpdateYAxisRange();
            }
        }

        // Helper to get the unit for each sensor
        private string GetUnitForSensor(string sensorName)
        {
            return sensorName switch
            {
                "Temperature" => "°C",
                "Humidity" => "%",
                "Pressure" => "hPa",
                _ => ""
            };
        }

        private void UpdateYAxisRange()
        {
            Sensor? selectedSensor = Sensors.FirstOrDefault(s => s.Name == SelectedSensor);

            if (selectedSensor != null && selectedSensor.Values.Any())
            {
                YAxes[0].MinLimit = selectedSensor.Values.Min() - 5;
                YAxes[0].MaxLimit = selectedSensor.Values.Max() + 5;
            }
            else if (selectedSensor != null)
            {
                YAxes[0].MinLimit = selectedSensor.MinValue;
                YAxes[0].MaxLimit = selectedSensor.MaxValue;
            }
        }

        // Method to calculate and update statistics
        private void UpdateStatistics()
        {
            Sensor? selectedSensor = Sensors.FirstOrDefault(s => s.Name == SelectedSensor);

            if (selectedSensor != null && selectedSensor.Values.Any())
            {
                List<double> values = selectedSensor.Values.ToList();

                Average = values.Average();
                MinValue = values.Min();
                MaxValue = values.Max();
                StandardDeviation = Math.Sqrt(values.Select(v => Math.Pow(v - Average, 2)).Average());
            }
            else
            {
                // Reset statistics if no data
                Average = 0;
                MinValue = 0;
                MaxValue = 0;
                StandardDeviation = 0;
            }
        }
        private void ShowNotification(string message, SolidColorBrush brush)
        {
            LoggingStatusMessage = message;
            LoggingStatusColor = brush;
            LoggingStatusVisibility = Visibility.Visible;

            _notificationTimer.Start();
        }

        private void CheckThresholds()
        {
            // Find the currently selected sensor
            Sensor? selectedSensor = Sensors.FirstOrDefault(s => s.Name == SelectedSensor);

            // Check if the selected sensor is breaching its threshold
            if (selectedSensor != null && selectedSensor.IsThresholdBreached())
            {
                AlertMessage = $"{selectedSensor.Name} value exceeded threshold! Current: {selectedSensor.CurrentValue:F2}, Threshold: {selectedSensor.Threshold}";

                // Restart the alert timer
                _alertTimer.Stop();
                _alertTimer.Start();

                return;
            }

            // Clear the alert if no thresholds are breached
            AlertMessage = string.Empty;
        }

        public async Task ConnectToAcquisitionServer()
        {
            try
            {
                Debug.WriteLine("Attempting to connect to server...");
                _client = new TcpClient();
                await _client.ConnectAsync("127.0.0.1", 5000);
                _stream = _client.GetStream();

                Debug.WriteLine("Connection established!");
                ShowNotification("Connected to Acquisition Server!", new SolidColorBrush(Colors.Green));

                _ = ReadSensorDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Connection failed: {ex.Message}");
                ShowNotification($"Error: {ex.Message}", new SolidColorBrush(Colors.Red));
            }
        }

        private async Task ReadSensorDataAsync()
        {
            byte[] buffer = new byte[1024];
            string leftover = string.Empty;

            while (_client.Connected)
            {
                try
                {
                    // Read data from the server
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Combine leftover data with the new data
                    data = leftover + data;

                    // Split messages by newline
                    string[] messages = data.Split('\n');

                    // Process all complete messages except the last one (it might be incomplete)
                    for (int i = 0; i < messages.Length - 1; i++)
                    {
                        try
                        {
                            Console.WriteLine($"Message to Deserialize: {messages[i]}");
                            SensorData? sensorData = JsonSerializer.Deserialize<SensorData>(messages[i]);
                            if (sensorData != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (SelectedSensor == "Temperature")
                                    {
                                        Sensors.First(s => s.Name == "Temperature").UpdateCurrentValue(sensorData.Temperature);
                                    }
                                    else if (SelectedSensor == "Humidity")
                                    {
                                        Sensors.First(s => s.Name == "Humidity").UpdateCurrentValue(sensorData.Humidity);
                                    }
                                    else if (SelectedSensor == "Pressure")
                                    {
                                        Sensors.First(s => s.Name == "Pressure").UpdateCurrentValue(sensorData.Pressure);
                                    }

                                    UpdateStatistics();
                                    UpdateYAxisRange();
                                });
                            }
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                            Console.WriteLine($"Failed Message: {messages[i]}");
                        }
                    }

                    // Save the last message as leftover (it may be incomplete)
                    leftover = messages[^1];
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ShowNotification("Disconnected from server!", new SolidColorBrush(Colors.Red));
                    break;
                }
            }
        }

        private async void StartAcquisition()
        {
            if (_client != null && _client.Connected)
            {
                try
                {
                    // Create a "StartAcquisition" command
                    Command command = new Command(CommandTypes.StartAcquisition);
                    string commandJson = JsonSerializer.Serialize(command);

                    // Send the command to the server
                    byte[] buffer = Encoding.UTF8.GetBytes(commandJson + "\n");
                    await _stream.WriteAsync(buffer, 0, buffer.Length);

                    ShowNotification("Acquisition started", new SolidColorBrush(Colors.Green));
                }
                catch (Exception ex)
                {
                    ShowNotification($"Error: {ex.Message}", new SolidColorBrush(Colors.Red));
                }
            }
        }

        private async void StopAcquisition()
        {
            if (_client != null && _client.Connected)
            {
                try
                {
                    // Create a "StopAcquisition" command
                    Command command = new Command(CommandTypes.StopAcquisition);
                    string commandJson = JsonSerializer.Serialize(command);

                    // Send the command to the server
                    byte[] buffer = Encoding.UTF8.GetBytes(commandJson + "\n");
                    await _stream.WriteAsync(buffer, 0, buffer.Length);

                    ShowNotification("Acquisition stopped", new SolidColorBrush(Colors.Red));
                }
                catch (Exception ex)
                {
                    ShowNotification($"Error: {ex.Message}", new SolidColorBrush(Colors.Red));
                }
            }
        }

        private bool CanSendCommand()
        {
            return _client != null && _client.Connected;
        }
        public void ShutdownAcquisitionServer()
        {
            try
            {
                if (_client == null)
                {
                    _client = new TcpClient("localhost", 5000); // Match the server's address and port
                }

                using NetworkStream stream = _client.GetStream();
                using StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                // Create the shutdown command
                Command shutDownCommand = new Command(CommandTypes.Shutdown);

                // Serialize and send the command
                string message = JsonSerializer.Serialize(shutDownCommand);
                writer.WriteLine(message);

                Console.WriteLine("Shutdown command sent to AcquisitionServer.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send shutdown command: {ex.Message}");
            }
            finally
            {
                _client?.Close();
                _client = null;
            }
        }
    }
}
