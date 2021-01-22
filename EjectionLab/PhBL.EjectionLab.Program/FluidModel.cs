using System;
using System.IO;
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
            double externalPressure
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

            await InputValue(machString, 0, 15, $"{workspaceFolder}/IDS.DAT");
            await InputValue(lavalAngleString, 16, 31, $"{workspaceFolder}/IDS.DAT");
            await InputValue(lavalEndPressureString, 32, 47, $"{workspaceFolder}/IDS.DAT");
            await InputValue(adiabaticIndexString + "\r\n", 48, 65, $"{workspaceFolder}/IDS.DAT");
            await InputValue(gasConstantString, 66, 81, $"{workspaceFolder}/IDS.DAT");
            await InputValue(lavalEndDiameterString + "\r\n", 83, 99, $"{workspaceFolder}/IDS.DAT");
            await InputValue(stopTemperatureString, 100, 115, $"{workspaceFolder}/IDS.DAT");
            await InputValue(externalPressureString, 216, 231, $"{workspaceFolder}/IDS.DAT");

            await RequestCalculation();

            return await GetFluidData($"{workspaceFolder}/REZ.DAT");
        }

        private async Task RequestCalculation()
        {
            throw new NotImplementedException();
        }

        private async Task<FluidData> GetFluidData(string resultsFilePath)
        {
            throw new NotImplementedException();
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