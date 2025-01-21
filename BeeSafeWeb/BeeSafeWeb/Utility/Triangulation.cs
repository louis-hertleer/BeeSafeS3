using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeeSafeWeb.Utility;

public class Triangle
{
    public Point[] Points { get; set; }

    /// <summary>
    /// Returns the centroid of the triangle.
    /// </summary>
    public Point Center
    {
        get
        {
            double x = (Points[0].X + Points[1].X + Points[2].X) / 3;
            double y = (Points[0].Y + Points[1].Y + Points[2].Y) / 3;
            return new Point(x, y);
        }
    }

    public Triangle(Point[] points)
    {
        Assert.AreEqual(points.Length, 3);
        Points = points;
    }
    
    /// <summary>
    /// Triangulates
    /// </summary>
    /// <param name="rays"></param>
    /// <returns>
    /// A circle, with the center being the center of the triangulation and the
    /// radius being the distance of the center to the furthest point of the
    /// triangle.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the number of rays is not 3.</exception>
    public static Circle? Triangulate(Ray[] rays)
    {
        /* only accept 3 points. */
        if (rays.Length != 3)
        {
            throw new ArgumentException("The number of rays must be 3.");
        }

        Point[] points = new Point[rays.Length];

        Triangle triangle = new Triangle(points);

        /* find the longest path to the center. */
        double length = triangle.Points.Select(p => p.Distance(triangle.Center))
                                       .ToList()
                                       .Max();

        return new Circle(length, triangle.Center);
    }
}