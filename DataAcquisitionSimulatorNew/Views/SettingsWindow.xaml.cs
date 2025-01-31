using DataAcquisitionSimulatorNew.Models;
using DataAcquisitionSimulatorNew.Services;
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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        private readonly SensorSettingsService _sensorSettingsService = new SensorSettingsService();

        public SettingsWindow()
        {
            InitializeComponent();

            DataContext = new SettingsViewModel();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {

            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Close the settings window
        }

    }
}
