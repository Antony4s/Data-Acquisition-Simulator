using DataAcquisitionSimulatorNew.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

namespace DataAcquisitionSimulatorNew.Services
{
    public static class ExcelService
    {
        static ExcelService()
        {
            // Ensure EPPlus is set to non-commercial license
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Exports the sensor data to an Excel file with proper headers, including units.
        /// </summary>
        public static void ExportToExcel(string filePath, List<Sensor> sensors)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sensor Data");

                // Write the header with units
                worksheet.Cells[1, 1].Value = "Timestamp";
                worksheet.Cells[1, 2].Value = "Temperature (°C)";
                worksheet.Cells[1, 3].Value = "Humidity (%)";
                worksheet.Cells[1, 4].Value = "Pressure (hPa)";

                // Add sensor data
                int row = 2; // Start at the second row
                for (int i = 0; i < sensors[0].Timestamps.Count; i++)
                {
                    worksheet.Cells[row, 1].Value = sensors[0].Timestamps[i]; // Timestamp
                    worksheet.Cells[row, 2].Value = sensors[0].Values[i];     // Temperature
                    worksheet.Cells[row, 3].Value = sensors[1].Values[i];     // Humidity
                    worksheet.Cells[row, 4].Value = sensors[2].Values[i];     // Pressure
                    row++;
                }

                // Auto-fit the columns for better readability
                worksheet.Cells.AutoFitColumns();

                // Save the Excel file
                package.SaveAs(new FileInfo(filePath));
            }
        }


        /// <summary>
        /// Imports sensor data from an Excel file.
        /// </summary>
        public static List<List<double>> ImportFromExcel(string filePath)
        {
            List<List<double>> data = new List<List<double>> { new List<double>(), new List<double>(), new List<double>() }; // Temperature, Humidity, Pressure
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                int row = 2; // Assuming first row is header
                while (worksheet.Cells[row, 1].Value != null)
                {
                    data[0].Add(double.Parse(worksheet.Cells[row, 2].Value.ToString())); // Temperature
                    data[1].Add(double.Parse(worksheet.Cells[row, 3].Value.ToString())); // Humidity
                    data[2].Add(double.Parse(worksheet.Cells[row, 4].Value.ToString())); // Pressure
                    row++;
                }
            }
            return data;
        }
    }
}
