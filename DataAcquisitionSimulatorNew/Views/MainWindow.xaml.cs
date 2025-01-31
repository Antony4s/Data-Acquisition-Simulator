using ClosedXML.Excel;
using DataAcquisitionSimulatorNew.Helpers;
using DataAcquisitionSimulatorNew.Models;
using DataAcquisitionSimulatorNew.Services;
using DataAcquisitionSimulatorNew.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace DataAcquisitionSimulatorNew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private readonly Timer _timer; // Timer to update sensor data
        //private readonly MainViewModel _viewModel;
        private readonly DataLoggerService _dataLoggerService;
        public MainWindow()
        {
            InitializeComponent();

            // Initialize DataLoggerService
            MainViewModel viewModel = new MainViewModel();
            _dataLoggerService = new DataLoggerService(viewModel.Sensors);

            // Bind the DataContext to MainViewModel
            DataContext = viewModel;
        }

        private void OnSimulationModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && DataContext is MainViewModel viewModel)
            {
                ComboBox? comboBox = sender as ComboBox;
                FakeSensorDataGenerator.SimulationMode selectedMode = (FakeSensorDataGenerator.SimulationMode)comboBox.SelectedItem;

                // Update the corresponding sensor
                viewModel.UpdateSensorMode("Temperature", selectedMode);
            }
        }

        private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string? selectedTheme = (e.AddedItems[0] as ComboBoxItem)?.Content.ToString();
                SetTheme(selectedTheme);
            }
        }

        public void SetTheme(string theme)
        {
            System.Collections.ObjectModel.Collection<ResourceDictionary> appResources = Application.Current.Resources.MergedDictionaries;

            // Clear existing resources
            appResources.Clear();

            // Apply the selected theme
            string themePath = theme == "Dark"
                ? "Resources/DarkTheme.xaml"
                : "Resources/Styles.xaml"; // Default is light
            appResources.Add(new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) });
        }
    }
}
