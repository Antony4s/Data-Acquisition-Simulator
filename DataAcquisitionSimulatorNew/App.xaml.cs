using DataAcquisitionSimulatorNew.Models;
using DataAcquisitionSimulatorNew.Services;
using DataAcquisitionSimulatorNew.ViewModels;
using DataAcquisitionSimulatorNew.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DataAcquisitionSimulatorNew
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainViewModel _mainViewModel;
        private ObservableCollection<Sensor> _sensors;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainViewModel = new MainViewModel();

        }

        protected override void OnExit(ExitEventArgs e)
        {
            SensorSettingsManager.Instance.SaveSettings();
            _mainViewModel.ShutdownAcquisitionServer(); // Send shutdown command
            base.OnExit(e);
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SensorSettingsManager.Instance.LoadSettings();

            // Show the splash screen
            SplashWindow splashWindow = new SplashWindow();
            splashWindow.Show();
        }
    }
}
