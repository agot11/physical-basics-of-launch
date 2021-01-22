using System;
using System.Threading.Tasks;

namespace PhBL.EjectionLab.Program
{
    public class CalculatingFunctions
    {
        public static async Task<double> GetMainPressure(double externalPressure, double ejectionLosses, double localLosses)
        {
            return await Task.FromResult(externalPressure - ejectionLosses - localLosses).ConfigureAwait(false);
        }

        public static async Task<double> GetEjectionLosses(double density, double ejectionVelocity)
        {
            return await Task.FromResult(density * Math.Pow(ejectionVelocity, 2) / 2).ConfigureAwait(false);
        }

        public static async Task<double> GetLocalLosses(double constrictionLosses, double expansionLosses, double rotationLosses)
        {
            return await Task.FromResult(constrictionLosses + expansionLosses + rotationLosses).ConfigureAwait(false);
        }

        // public static async Task<double> GetEjectionVelocity()
    }
}