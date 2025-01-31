using DataAcquisitionSimulatorNew.Models;
using DataAcquisitionSimulatorNew.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DataAcquisitionSimulatorNew.Views
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {

        public SplashWindow()
        {
            InitializeComponent();

            DataContext = new SplashWindowViewModel();
        }

        private void GoToVisualization_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close(); // Close the splash screen
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog(); // Show as a modal dialog
        }

        private void OpenSettings()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
                Application.Current.MainWindow.Close(); // Close SplashWindow
            });
        }

        private void OpenVisualization()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Application.Current.MainWindow.Close(); // Close SplashWindow
            });
        }
    }
}
