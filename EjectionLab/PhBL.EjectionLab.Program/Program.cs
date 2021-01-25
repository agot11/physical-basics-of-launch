using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using ScottPlot;

namespace PhBL.EjectionLab.Program
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var fluidModel = new FluidModel();
            var lossesModel = new LossesModel();

            var lavalEndDiameter = 0.5;
            var sternDiameter = 0.6;
            var tubeDiameter = 0.7;

            // var lavalEndDiameter = 1;
            // var sternDiameter = 1.1;
            // var tubeDiameter = 1.15;

            // var lavalEndDiameter = 0.8;
            // var sternDiameter = 1;
            // var tubeDiameter = 1.2;

            var mach = 3;
            var lavalAngle = 10;
            var lavalEndPressure = 0.8 * 10e5;
            var fluidData = new FluidData
            {
                MaxDiameter = 0,
                LavalEndPressure = 0.8 * 10e5,
                Geometry = new(double coordinated, double radius) [0]
            };
            var adiabaticIndex = 1.4;
            var gasConstant = 287;
            var stopTemperature = 273;
            var externalPressure = 10e5;
            var lavalEndSquare = 0.5;
            var ejectionAreaLength = 7;
            var airDensity = 1.2041;
            (double Ejection, double Local) losses;

            var geometryData = new List<(double Coordinate, double RadiusBorder)[]>();
            var externalPressureData = new List<double>();
            var ejectionLossesData = new List<double>();
            var localLossesData = new List<double>();

            while (fluidData.MaxDiameter < tubeDiameter)
            {
                fluidData = await fluidModel.CalculateFluid(
                    mach: mach, //3
                    lavalAngle : lavalAngle, //10
                    lavalEndPressure :  lavalEndPressure, // 0.8*10e5,
                    adiabaticIndex : adiabaticIndex, // 1.4,
                    gasConstant : gasConstant, // 287,
                    lavalEndDiameter : lavalEndDiameter, //0.8
                    stopTemperature : stopTemperature, //293,
                    externalPressure : externalPressure, //10e5
                    calculatingLength : ejectionAreaLength // 5
                ).ConfigureAwait(false);

                losses = await lossesModel.CalculateLosses(
                    lavalEndPressure: lavalEndPressure, //0.8*10^5
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

                externalPressure -= losses.Ejection + losses.Local;

                geometryData.Add(fluidData.Geometry);
                externalPressureData.Add(externalPressure);
                ejectionLossesData.Add(losses.Ejection);
                localLossesData.Add(losses.Local);
            }

            var newEjectionAreaLength = fluidData.Geometry.MinBy(coord => Math.Abs(coord.RadiusBorder * 2 - fluidData.MaxDiameter)).FirstOrDefault().Coordinate;

            for (int i = 0; i < 6; i++)
            {
                fluidData = await fluidModel.CalculateFluid(
                    mach: mach, //3
                    lavalAngle : lavalAngle, //10
                    lavalEndPressure : lavalEndPressure, // 0.8*10e5,
                    adiabaticIndex : adiabaticIndex, // 1.4,
                    gasConstant : gasConstant, // 287,
                    lavalEndDiameter : lavalEndDiameter, //0.8
                    stopTemperature : stopTemperature, //293,
                    externalPressure : externalPressure, //10e5,
                    calculatingLength : ejectionAreaLength // crossing coordinate
                ).ConfigureAwait(false);

                losses = await lossesModel.CalculateLosses(
                    lavalEndPressure: lavalEndPressure, //0.8*10^5
                    externalPressure : externalPressure, //10^5 on init and calculated every iteration
                    fluidGeometry : fluidData.Geometry, // new (double coordinate, double radius)[1], // from fluid model
                    mach : mach, //3
                    adiabaticIndex : adiabaticIndex, //1.4
                    gasConstant : gasConstant, //287
                    stopTemperature : stopTemperature, //293
                    lavalEndSquare : lavalEndSquare, //0.5
                    ejectionAreaLength : newEjectionAreaLength, //5
                    airDensity : airDensity, //1.2041
                    tubeDiameter : tubeDiameter, //1.2
                    sternDiameter : sternDiameter //1
                ).ConfigureAwait(false);

                externalPressure -= losses.Ejection + losses.Local;

                geometryData.Add(fluidData.Geometry);
                externalPressureData.Add(externalPressure);
                ejectionLossesData.Add(losses.Ejection);
                localLossesData.Add(losses.Local);
            }

            // await SaveData(geometryData, nameof(geometryData));

            await PlotDataWithIterations(externalPressureData.ToArray(), "Изменение внешнего давления", "Давление в приструйной зоне, Па", "Номер итерации", "./Pext.png");
            await PlotDataWithIterations(ejectionLossesData.ToArray(), "Изменение потерь на эжекцию воздуха", "Величина потерь, Па", "Номер итерации", "./Hej.png");
            await PlotDataWithIterations(localLossesData.ToArray(), "Изменение потерь на местные сопротивления", "Величина потерь, Па", "Номер итерации", "./Hl.png");

            await PlotFluidGeometry(geometryData.ToArray(), "Изменение структуры струи", "Радиус границы, м", "Осевая координата, м", "./FluidStruct.png");
        }

        private static Task SaveData(List<(double Coordinate, double RadiusBorder)[]> geometryData, string fileName)
        {
            throw new NotImplementedException();
        }

        private static async Task PlotDataWithIterations(double[] data, string title, string yL, string xL, string savingPath)
        {
            var x = Enumerable.Range(1, data.Length).Select(Convert.ToDouble).ToArray();

            var plot = new Plot();
            plot.PlotScatter(x, data, label: title);
            plot.Title(title);
            plot.XLabel(xL);
            plot.YLabel(yL);
            plot.Legend();

            plot.SaveFig(savingPath);
        }

        private static async Task PlotFluidGeometry((double x, double y)[][] data, string title, string yL, string xL, string savingPath)
        {
            var plot = new Plot();

            for (int i = 0; i < data.Length; i++)
            {
                if (i % 2 > 0 && i != 0 && i != data.Length - 1)
                    continue;

                var x = data[i].Select(data => data.x).ToArray();
                var y = data[i].Select(data => data.y).ToArray();
                Color randomColor = Color.FromArgb(new Random().Next(256), new Random().Next(256), new Random().Next(256));

                plot.PlotScatter(x, y, label: $"Итерация {i}", color: randomColor);
            }

            plot.AxisZoom(1, 1);
            plot.Title(title);
            plot.XLabel(xL);
            plot.YLabel(yL);
            plot.Legend();

            plot.SaveFig(savingPath);
        }
    }
}