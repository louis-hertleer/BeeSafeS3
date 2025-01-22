namespace BeeSafeWeb.Utility.Geometry;

public class Circle
{
    public double Radius { get; set; }
    public Point Center { get; set; }
    public double Circumference => Math.PI * Radius * 2;
    
    public Circle(double radius, Point center)
    {
        Radius = radius;
        Center = center;
    }
}