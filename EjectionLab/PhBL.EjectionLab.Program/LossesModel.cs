using System;
using System.Threading.Tasks;

namespace PhBL.EjectionLab.Program
{
    internal class LossesModel
    {
        public LossesModel()
        {
        }

        internal async Task<double> CalculateLosses(double lavalEndPressure, double externalPressure, (double coordinate, double radius)[] fluidGeometry, double mach, double adiabaticIndex, double gasConstant, double stopTemperature, double lavalEndSquare, double ejectionAreaLength, double airDensity, double tubeDiameter, double sternDiameter)
        {
            // throw new NotImplementedException();R
            return new double();
        }
    }
}