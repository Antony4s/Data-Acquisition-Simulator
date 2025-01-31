
using AcquisitionShared.Protocol;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json; // For JSON deserialization

namespace AcquisitionServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Acquisition Server...");

            // Set up the TCP listener
            TcpListener listener = new TcpListener(IPAddress.Any, 5000); // Listening on port 5000
            listener.Start();
            Console.WriteLine("Server is listening on port 5000...");

            while (true)
            {
                // Accept incoming client connections
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");

                // Handle the client connection in a separate task
                _ = HandleClientAsync(client);
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            using NetworkStream stream = client.GetStream();
            FakeSensorDataGenerator fakeDataGenerator = new FakeSensorDataGenerator();
            bool isAcquisitionRunning = false;

            try
            {
                while (client.Connected)
                {
                    // Check if there's a command from the client
                    if (stream.DataAvailable)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        string commandJson = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                        try
                        {
                            // Deserialize the command JSON
                            Command? command = JsonSerializer.Deserialize<Command>(commandJson);
                            if (command != null)
                            {
                                // Handle the command based on its type
                                switch (command.Type)
                                {
                                    case CommandTypes.StartAcquisition:
                                        isAcquisitionRunning = true;
                                        Console.WriteLine("Acquisition started.");
                                        break;

                                    case CommandTypes.StopAcquisition:
                                        isAcquisitionRunning = false;
                                        Console.WriteLine("Acquisition stopped.");
                                        break;

                                    case CommandTypes.Shutdown:
                                        Console.WriteLine("Shutdown command received. Stopping the server...");
                                        Environment.Exit(0); // Gracefully terminate the server
                                        break;

                                    default:
                                        Console.WriteLine($"Unknown command type: {command.Type}");
                                        break;
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Invalid command received: {ex.Message}");
                        }
                    }

                    // Send data only if acquisition is running
                    if (isAcquisitionRunning)
                    {
                        double temperature = fakeDataGenerator.GenerateTemperatureData();
                        double humidity = fakeDataGenerator.GenerateHumidityData();
                        double pressure = fakeDataGenerator.GeneratePressureData();

                        string data = string.Format(CultureInfo.InvariantCulture,
                        "{{\"Temperature\":{0:F2},\"Humidity\":{1:F2},\"Pressure\":{2:F2}}}\n",
                        temperature, humidity, pressure);
                        byte[] buffer = Encoding.UTF8.GetBytes(data);
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                        Console.WriteLine($"Sent: {data}");

                        await Task.Delay(1000); // Adjust delay for simulation speed
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Client disconnected.");
        }


        // FakeSensorDataGenerator simulates sensor values
        public class FakeSensorDataGenerator
        {
            private readonly Random _random = new Random();

            public double GenerateTemperatureData() => Math.Round(_random.NextDouble() * 50, 2); // 0 to 50 °C
            public double GenerateHumidityData() => Math.Round(_random.NextDouble() * 100, 2);  // 0 to 100 %
            public double GeneratePressureData() => Math.Round(950 + _random.NextDouble() * 100, 2); // 950 to 1050 hPa
        }
    }
}