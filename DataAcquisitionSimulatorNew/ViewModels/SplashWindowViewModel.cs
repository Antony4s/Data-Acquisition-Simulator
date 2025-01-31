using DataAcquisitionSimulatorNew.Helpers;
using DataAcquisitionSimulatorNew.Models;
using DataAcquisitionSimulatorNew.ViewModels;
using DataAcquisitionSimulatorNew.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DataAcquisitionSimulatorNew.ViewModels
{
    public class SplashWindowViewModel
    {
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenVisualizationCommand { get; }

        private ObservableCollection<Sensor> _sensors;

        public SplashWindowViewModel()
        {


            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenVisualizationCommand = new RelayCommand(OpenVisualization);
        }

        private void OpenSettings()
        {
            // Open the settings window
            Application.Current.Dispatcher.Invoke(() =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            });
        }

        private void OpenVisualization()
        {
            // Open the main visualization window
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            });
        }
    }
}
