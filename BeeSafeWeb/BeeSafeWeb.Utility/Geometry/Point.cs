namespace BeeSafeWeb.Utility.Geometry;

/// <summary>
/// A point on a Euclidean plane.
/// </summary>
public class Point
{
    public double X { get; set; }
    public double Y { get; set; }

    public Point()
    {
    }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double Distance(Point p)
    {
        return Math.Sqrt(Math.Pow(p.X - X, 2) + Math.Pow(p.Y - Y, 2));
    }
}