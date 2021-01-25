using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

namespace PhBL.EjectionLab.Program
{
    internal class LossesModel
    {
        public LossesModel()
        {
        }

        internal async Task<(double Ejection, double Local)> CalculateLosses(
            double lavalEndPressure,
            double externalPressure,
            (double Coordinate, double Radius)[] fluidGeometry,
            double mach,
            double adiabaticIndex,
            double gasConstant,
            double stopTemperature,
            double lavalEndSquare,
            double ejectionAreaLength,
            double airDensity,
            double tubeDiameter,
            double sternDiameter
        )
        {
            // throw new NotImplementedException();R
            var ejectionLosses = await CalculateEjectionLosses(
                adiabaticIndex,
                gasConstant,
                mach,
                stopTemperature,
                lavalEndPressure,
                externalPressure,
                fluidGeometry,
                airDensity,
                lavalEndSquare
            );

            var localLosses = await CalculateLocalLosses(
                fluidGeometry,
                tubeDiameter,
                sternDiameter,
                ejectionLosses.AvgEjectionVelocity
            ).ConfigureAwait(false);

            return (Ejection: ejectionLosses.Losses, Local: localLosses);
        }

        private async Task<(double Losses, double AvgEjectionVelocity)> CalculateEjectionLosses(
            double adiabaticIndex,
            double gasConstant,
            double mach,
            double stopTemperature,
            double lavalEndPressure,
            double externalPressure,
            (double Coordinate, double Radius)[] fluidGeometry,
            double airDensity,
            double lavalEndSquare
        )
        {
            var sonicCoordinate = (((13.28 * Math.Sqrt(adiabaticIndex * Math.Pow(mach, 2))) + 11.8) * Math.Pow(lavalEndPressure / externalPressure, 0.33)) - 23.57;

            var sonicVelocity = Math.Sqrt(adiabaticIndex / (gasConstant * stopTemperature)) * externalPressure * Math.Sqrt((adiabaticIndex + 1) / 2);
            var sonicSectionRadius = fluidGeometry.MinBy(coord => Math.Abs(coord.Coordinate - sonicCoordinate)).FirstOrDefault().Radius;
            var sonicSectionExpense = 0.217 * Math.PI * sonicVelocity * sonicSectionRadius;
            
            var lavalCoef1 = 0.98;
            var lavalCoef2 = Math.Pow(2 / (adiabaticIndex + 1), (adiabaticIndex + 1) / (2 * (adiabaticIndex - 1)));
            var lavalCriticalSection = lavalEndSquare / ((1 / Math.Pow(mach, 2)) * (2 / (adiabaticIndex + 1)) * Math.Pow((1 + (Math.Pow(mach, 2) * (adiabaticIndex - 1) / 2)), ((adiabaticIndex + 1) / (adiabaticIndex - 1))));
            var initCameraPressure = lavalEndPressure * Math.Pow(1 + ((adiabaticIndex - 1) * Math.Pow(mach, 2) / 2), adiabaticIndex / (adiabaticIndex - 1));
            var idealExpense = lavalCoef1 * (lavalCriticalSection * lavalCoef2 * initCameraPressure / Math.Sqrt(gasConstant * stopTemperature));

            var ejectionAbility = (sonicSectionExpense - idealExpense) / sonicCoordinate;

            var ejectionLosses = new List<double>();
            var ejectionVelocities = new List<double>();
            foreach (var (Coordinate, Radius) in fluidGeometry)
            {
                var ejectionVelocity = ejectionAbility / (2 * Math.PI * airDensity * Radius);
                ejectionLosses.Add(airDensity * Math.Pow(ejectionVelocity, 2) / 2);
                ejectionVelocities.Add(ejectionVelocity);
            }

            var losses = ejectionLosses.Aggregate((loss1, loss2) => loss1 + loss2);
            var avgEjectionVelocity = ejectionVelocities.Aggregate((vel1, vel2) => vel1+ vel2) / ejectionVelocities.Count;
            return await Task.FromResult((losses, avgEjectionVelocity)).ConfigureAwait(false);
        }

        private async Task<double> CalculateLocalLosses(
            (double Coordinate, double Radius)[] fluidGeometry,
            double tubeDiameter,
            double sternDiameter,
            double avgEjectionVelocity)
        {
            var onFirstSquare = Math.PI * (Math.Pow(tubeDiameter, 2) - Math.Pow(sternDiameter, 2)) / 4;
            var onSecondSquare = Math.PI * (Math.Pow(tubeDiameter, 2) - Math.Pow(fluidGeometry[0].Radius * 2, 2)) / 4;
            var velocityOnFirst = avgEjectionVelocity * onSecondSquare / onFirstSquare;
            var velocityOnSecond = onFirstSquare * velocityOnFirst / onSecondSquare;

            var constrictionCoef = 0.5;
            var constrictionLosses = constrictionCoef * Math.Pow(velocityOnFirst, 2) / (2 * 9.81);

            var expansionCoef = Math.Pow(1 - (onFirstSquare / onSecondSquare), 2);
            var expansionLosses = expansionCoef * Math.Pow(velocityOnSecond, 2) / (2 * 9.81);

            var rotationLosses = new List<double>();

            for (int i = 1; i < fluidGeometry.Length; i++)
            {
                var radiusInc = fluidGeometry[i].Radius - fluidGeometry[i-1].Radius;
                var coordInc = fluidGeometry[i].Coordinate - fluidGeometry[i-1].Coordinate;
                var rotationAngle = Math.Atan(radiusInc / coordInc);
                var rotationCoef = (0.95 * Math.Pow(Math.Sin(rotationAngle / 2), 2)) + (2.05 * Math.Pow(Math.Sin(rotationAngle / 2), 4));
                var velocityOnThird = avgEjectionVelocity;

                rotationLosses.Add(rotationCoef * Math.Pow(velocityOnThird, 2) / (2 * 9.81));
            }

            return constrictionLosses + expansionLosses + rotationLosses.Aggregate((loss1, loss2) => loss1 + loss2);
        }
    }
}