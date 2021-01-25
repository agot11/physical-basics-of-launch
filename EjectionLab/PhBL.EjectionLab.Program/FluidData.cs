namespace PhBL.EjectionLab.Program
{
    internal class FluidData
    {
        public FluidData()
        {
        }

        public double MaxDiameter { get; internal set; }
        public double LavalEndPressure { get; internal set; }
        public (double Coordinate, double RadiusBorder)[] Geometry { get; internal set; }
    }
}