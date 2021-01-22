using System;
using System.Threading.Tasks;

using ScottPlot;

namespace PhBL.EjectionLab.Program
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var fluidModel = new FluidModel();
            var lossesModel = new LossesModel();

            var lavalEndDiameter = 0.8;
            var sternDiameter = 1;
            var tubeDiameter = 1.2;

            var mach = 3;
            var lavalAngle = 10;
            var fluidData = new FluidData
            {
                MaxDiameter = 0,
                LavalEndPressure = 0.8 * 10e5,
                Geometry = new(double coordinated, double radius) [0]
            };
            var adiabaticIndex = 1.4;
            var gasConstant = 287;
            var stopTemperature = 293;
            var externalPressure = 10e5;
            var lavalEndSquare = 0.5;
            var ejectionAreaLength = 5;
            var airDensity = 1.2041;
            double losses = 0;

            while (fluidData.MaxDiameter < tubeDiameter)
            {
                fluidData = await fluidModel.CalculateFluid(
                    mach: mach, //3
                    lavalAngle : lavalAngle, //10
                    lavalEndPressure : fluidData.LavalEndPressure, // 0.8*10e5,
                    adiabaticIndex : adiabaticIndex, // 1.4,
                    gasConstant : gasConstant, // 287,
                    lavalEndDiameter : lavalEndDiameter, //0.8
                    stopTemperature : stopTemperature, //293,
                    externalPressure : externalPressure //10e5
                ).ConfigureAwait(false);

                losses = await lossesModel.CalculateLosses(
                    lavalEndPressure: fluidData.LavalEndPressure, //0.8*10^5
                    externalPressure : externalPressure, //10^5 on init and calculated every iteration
                    fluidGeometry : fluidData.Geometry, // new (double coordinate, double radius)[1], // from fluid model
                    mach : mach, //3
                    adiabaticIndex : adiabaticIndex, //1.4
                    gasConstant : gasConstant, //287
                    stopTemperature : stopTemperature, //293
                    lavalEndSquare : lavalEndSquare, //0.5
                    ejectionAreaLength : ejectionAreaLength, //5
                    airDensity : airDensity, //1.2041
                    tubeDiameter : tubeDiameter, //1.2
                    sternDiameter : sternDiameter //1
                ).ConfigureAwait(false);

                externalPressure -= losses;
            }
 
            for (int i = 0; i < 6; i++)
            {
                fluidData = await fluidModel.CalculateFluid(
                    mach: mach, //3
                    lavalAngle : lavalAngle, //10
                    lavalEndPressure : fluidData.LavalEndPressure, // 0.8*10e5,
                    adiabaticIndex : adiabaticIndex, // 1.4,
                    gasConstant : gasConstant, // 287,
                    lavalEndDiameter : lavalEndDiameter, //0.8
                    stopTemperature : stopTemperature, //293,
                    externalPressure : externalPressure //10e5
                ).ConfigureAwait(false);

                losses = await lossesModel.CalculateLosses(
                    lavalEndPressure: fluidData.LavalEndPressure, //0.8*10^5
                    externalPressure : externalPressure, //10^5 on init and calculated every iteration
                    fluidGeometry : fluidData.Geometry, // new (double coordinate, double radius)[1], // from fluid model
                    mach : mach, //3
                    adiabaticIndex : adiabaticIndex, //1.4
                    gasConstant : gasConstant, //287
                    stopTemperature : stopTemperature, //293
                    lavalEndSquare : lavalEndSquare, //0.5
                    ejectionAreaLength : ejectionAreaLength, //5
                    airDensity : airDensity, //1.2041
                    tubeDiameter : tubeDiameter, //1.2
                    sternDiameter : sternDiameter //1
                ).ConfigureAwait(false);

                externalPressure -= losses;
            }
        }
    }
}