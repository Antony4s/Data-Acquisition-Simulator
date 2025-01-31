using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcquisitionSimulatorNew.Helpers
{
    public class FakeSensorDataGenerator
    {
        private readonly Random _random;

        private double _sinCounter = 0; // For sinusoidal mode
        public SimulationMode CurrentMode { get; set; } = SimulationMode.Linear;
        public enum SimulationMode
        {
            Linear,
            Sinusoidal,
            RandomSpikes,
            Flatline
        }

        public FakeSensorDataGenerator()
        {
            _random = new Random();
        }


        public double GenerateValue(double currentValue, double minValue, double maxValue, double trendStep)
        {
            switch (CurrentMode)
            {
                case SimulationMode.Linear:
                    return GenerateLinearValue(currentValue, minValue, maxValue, trendStep);

                case SimulationMode.Sinusoidal:
                    return GenerateSinusoidalValue(minValue, maxValue);

                case SimulationMode.RandomSpikes:
                    return GenerateRandomSpikeValue(currentValue, minValue, maxValue);

                case SimulationMode.Flatline:
                    return currentValue; // Flatline stays constant
            }

            return currentValue; // Default fallback
        }

        public double GenerateLinearValue(double currentValue, double minValue, double maxValue, double trendStep)
        {
            double newValue = currentValue + trendStep;
            return Math.Clamp(newValue, minValue, maxValue);
        }

        public double GenerateSinusoidalValue(double minValue, double maxValue)
        {
            _sinCounter += 0.1; // Adjust frequency
            double amplitude = (maxValue - minValue) / 2;
            double offset = (maxValue + minValue) / 2;
            return amplitude * Math.Sin(_sinCounter) + offset;
        }

        public double GenerateRandomSpikeValue(double currentValue, double minValue, double maxValue)
        {
            double spike = _random.NextDouble() > 0.8 ? _random.NextDouble() * (maxValue - minValue) : 0;
            return Math.Clamp(currentValue + spike - (spike / 2), minValue, maxValue);
        }

        public double GenerateRandomValue(double min, double max)
        {
            // Generate a random value within the specified range
            return Math.Round(_random.NextDouble() * (max - min) + min, 2);
        }

        public double GenerateTrendingValue(double currentValue, double min, double max, double step)
        {
            double trend = (_random.NextDouble() - 0.5) * step;
            double newValue = Math.Clamp(currentValue + trend, min, max);
            Debug.WriteLine($"[Trending] {currentValue} -> {newValue} (Min: {min}, Max: {max}, Step: {step})");
            return Math.Round(newValue, 2);
        }

        public double GenerateValueWithNoise(double baseValue, double noiseLevel)
        {
            double noise = (_random.NextDouble() - 0.5) * noiseLevel;
            double newValue = baseValue + noise;
            Debug.WriteLine($"[Noise] {baseValue} -> {newValue} (Noise Level: {noiseLevel})");
            return Math.Round(newValue, 2);
        }
    }
}
