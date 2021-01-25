using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhBL.EjectionLab.Program
{
    internal class FluidModel
    {
        // private string executablePath = "./FluidCalculationApp/Str_st.exe";
        private string workspaceFolder = "./FluidCalculationApp";

        public FluidModel()
        {
        }

        internal async Task<FluidData> CalculateFluid(
            double mach,
            double lavalAngle,
            double lavalEndPressure,
            double adiabaticIndex,
            double gasConstant,
            double lavalEndDiameter,
            double stopTemperature,
            double externalPressure,
            double calculatingLength
        )
        {
            var machString = mach.ToString("E6");
            var lavalAngleString = (lavalAngle * Math.PI / 180).ToString("E6");
            var lavalEndPressureString = (lavalEndPressure / 10e5).ToString("E6");
            var adiabaticIndexString = adiabaticIndex.ToString("E6");
            var gasConstantString = gasConstant.ToString("E6");
            var lavalEndDiameterString = lavalEndDiameter.ToString("E6");
            var stopTemperatureString = stopTemperature.ToString("E6");
            var externalPressureString = (externalPressure / 10e5).ToString("E6");

            var sectionStopTempString = 3300.ToString("E6");
            var gasConst346 = 346.ToString("E6");

            await InputValue(machString, 0, 15, $"{workspaceFolder}/IDS.DAT");
            await InputValue(lavalAngleString, 16, 31, $"{workspaceFolder}/IDS.DAT");
            await InputValue(lavalEndPressureString, 32, 47, $"{workspaceFolder}/IDS.DAT");
            await InputValue(adiabaticIndexString + "\r\n", 48, 65, $"{workspaceFolder}/IDS.DAT");
            await InputValue(gasConst346, 66, 81, $"{workspaceFolder}/IDS.DAT");
            await InputValue(lavalEndDiameterString + "\r\n", 83, 99, $"{workspaceFolder}/IDS.DAT");
            // await InputValue(stopTemperatureString, 100, 115, $"{workspaceFolder}/IDS.DAT");
            await InputValue(sectionStopTempString, 100, 115, $"{workspaceFolder}/IDS.DAT");
            await InputValue(adiabaticIndexString, 116, 131, $"{workspaceFolder}/IDS.DAT");
            await InputValue(adiabaticIndexString, 166, 181, $"{workspaceFolder}/IDS.DAT");
            await InputValue(externalPressureString, 216, 231, $"{workspaceFolder}/IDS.DAT");
            await InputValue(stopTemperatureString, 232, 247, $"{workspaceFolder}/IDS.DAT");
            await InputValue(gasConstantString + "\r\n", 264, 281, $"{workspaceFolder}/IDS.DAT");

            
            

            await RequestCalculation();

            return await GetFluidData($"{workspaceFolder}/REZ.DAT", calculatingLength);
        }

        private async Task RequestCalculation()
        {
            using var process = new Process();
            process.StartInfo.WorkingDirectory = workspaceFolder;
            process.StartInfo.FileName = $"{workspaceFolder}/Str_st.exe";
            process.Start();
            await Task.Delay(500).ConfigureAwait(false);

            var windowHandle = WinuserInvoke.FindWindow(null, "Str_st - [Расчет параметров струй ]");
            var systemMenuHandle = WinuserInvoke.GetSystemMenu(windowHandle, false);
            var systemMenuItemId = WinuserInvoke.GetMenuItemID(systemMenuHandle, 0);
            WinuserInvoke.SendMessage(windowHandle, WinuserInvoke.WM_COMMAND, (int)systemMenuItemId, (int)systemMenuHandle);

            windowHandle = WinuserInvoke.FindWindow(null, "Str_st");
            var windowMenuHandle = WinuserInvoke.GetMenu(windowHandle);
            var windowMenuItemId = WinuserInvoke.GetMenuItemID(windowMenuHandle, 1);
            WinuserInvoke.PostMessage(windowHandle, WinuserInvoke.WM_COMMAND, (int)windowMenuItemId, (int)windowMenuHandle);
            await Task.Delay(500).ConfigureAwait(false);

            IntPtr initialDataWindowHandle = IntPtr.Zero;
            while(initialDataWindowHandle == IntPtr.Zero)
            {
                initialDataWindowHandle = WinuserInvoke.FindWindow(null, "Исходные данные");
                await Task.Delay(50).ConfigureAwait(false);
            }
            var okButton = WinuserInvoke.FindWindowEx(initialDataWindowHandle, IntPtr.Zero, null, "Ok");
            var okButtonId = WinuserInvoke.GetDlgCtrlID(okButton);
            WinuserInvoke.SendMessage(initialDataWindowHandle, WinuserInvoke.WM_COMMAND, okButtonId, (int)okButton);
            await Task.Delay(200).ConfigureAwait(false);

            var profileHandle = WinuserInvoke.FindWindow(null, "Запрос");
            if (profileHandle != IntPtr.Zero)
            {
                WinuserInvoke.SendMessage(profileHandle, WinuserInvoke.WM_COMMAND, 6, profileHandle.ToInt32());
            }
            await Task.Delay(200).ConfigureAwait(false);

            var subMenuHandle = WinuserInvoke.GetSubMenu(windowMenuHandle, 2);
            var subMenuItemId = WinuserInvoke.GetMenuItemID(subMenuHandle, 0);
            WinuserInvoke.PostMessage(windowHandle, WinuserInvoke.WM_COMMAND, (int)subMenuItemId, (int)subMenuHandle);
            await Task.Delay(800).ConfigureAwait(false);

            process.Kill();
            await Task.Delay(500).ConfigureAwait(false);
        }

        private async Task<FluidData> GetFluidData(string resultsFilePath, double calcutatingLenght)
        {
            var fluidData = new FluidData();

            var textResult = await File.ReadAllTextAsync(resultsFilePath);
            var sections = textResult.Split('X', StringSplitOptions.RemoveEmptyEntries);

            var firstSection = sections[0];
            var firstSectionData = await GetSectionData(firstSection);
            sections = sections.Where(section => section != firstSection).ToArray();

            // fluidData.Geometry = new (double coordinate, double radius)[sections.Length];
            fluidData.LavalEndPressure = firstSectionData.AxisPressure;
            fluidData.MaxDiameter = 0;

            var geometryList = new List<(double Coordinate, double RadiusBorder)>();
            foreach (var section in sections)
            {
                var sectionData = await GetSectionData(section);

                if (sectionData.Coordinate > calcutatingLenght)
                    break;

                geometryList.Add((sectionData.Coordinate, sectionData.RadiusBorder));

                if (sectionData.RadiusBorder * 2 > fluidData.MaxDiameter)
                    fluidData.MaxDiameter = sectionData.RadiusBorder * 2;
            }

            fluidData.Geometry = geometryList.ToArray();

            return fluidData;
        }

        private async Task<(double Coordinate, double RadiusBorder, double RadiusWave, double AxisPressure, double BarrelNum)> GetSectionData(string section)
        {
            var lines = section.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

            var geometryData = lines[0].Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            var pressure = double.Parse(lines[1].Split(new char[0], StringSplitOptions.RemoveEmptyEntries)[1]) * 98066.5; // to Pascals

            return await Task.FromResult(
            (
                Coordinate: double.Parse(geometryData[0]),
                RadiusBorder: double.Parse(geometryData[3]),
                RadiusWave: double.Parse(geometryData[2]),
                AxisPressure: pressure,
                BarrelNum: double.Parse(geometryData[1])
            ));
        }

        private async Task InputValue(string value, int initPosition, int endPosition, string filePath)
        {
            var initialText = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            // var initialLenght = initialText.Length;
            var textArray = initialText.ToCharArray();
            var valueArray = value.ToCharArray();

            for (int i = initPosition; i < endPosition; i++)
            {
                textArray[i] = ' ';
            }

            for (int i = 0; i < valueArray.Length; i++)
            {
                textArray[endPosition - valueArray.Length + i + 1] = valueArray[i];
            }

            await File.WriteAllTextAsync(filePath, new string(textArray)).ConfigureAwait(false);
        }
    }
}