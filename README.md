# Data Acquisition Simulator
Data Acquisition Simulator is a WPF-based desktop application designed for simulating, visualizing, and logging sensor data. It features:

## Features
Real-Time Sensor Simulation: Simulates Temperature, Humidity, and Pressure data with customizable parameters like trend step, noise level, and simulation mode (e.g., Linear, Sinusoidal, Random Spikes).
Dynamic Chart Visualization: Displays sensor data in real-time using LiveCharts.
Data Logging and Export: Logs sensor data to CSV or Excel for further analysis.
Acquisition Server Integration: Communicates with an external acquisition server via TCP to simulate real-world data acquisition.
Modular Design: Implements MVVM architecture, making it maintainable and scalable.
Dark and Light Themes: Offers a professional UI with theme customization.
Settings Management: Saves sensor configurations for persistent and user-friendly customization.

---

## How to Install/Use

### Prerequisites
1. **Windows OS** with .NET Runtime installed (preferably .NET 6.0 or later).
2. Visual Studio (if you plan to compile the project).

### Installation
1. **Clone the repository**:
   
   git clone https://github.com/yourusername/DataAcquisitionSimulator.git

2. **Open the solution** in Visual Studio.

3. **Build the solution**:
   - Ensure the solution builds successfully.
   - Restore NuGet packages if prompted.

4. **Run the application**:
   - Set `DataAcquisitionSimulatorNew` as the startup project.

### Usage
1. **Start the Acquisition Server**:
   - Navigate to the `AcquisitionServer` folder in the project.
   - Build and run the server to start sending simulated data.

2. **Launch the Application**:
   - Upon launch, you'll be greeted with a splash screen.
   - Choose to configure sensor settings (yet to be implemented) or go directly to data visualization.

3. **Customize Settings**:
   - Adjust parameters like noise level, trend step, and simulation mode in the settings window.

4. **Visualize Data**:
   - View real-time data updates and interact with the visualization controls.
   - Use the Start/Stop commands for acquisition and logging as needed.

5. **Save Data**:
   - Log data to a CSV or Excel file for analysis.

## Contributing

1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature-name
   ```
3. Commit changes and push:
   ```bash
   git commit -m "Add a new feature"
   git push origin feature-name
   ```
4. Submit a Pull Request.

---

## License

This project is licensed under the [MIT License](LICENSE).

---



