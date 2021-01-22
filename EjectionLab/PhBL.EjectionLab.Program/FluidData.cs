namespace PhBL.EjectionLab.Program
{
    internal class FluidData
    {
        public FluidData()
        {
        }

        public double MaxDiameter { get; internal set; }
        public double LavalEndPressure { get; internal set; }
        public (double coordinate, double radius)[] Geometry { get; internal set; }
    }
}