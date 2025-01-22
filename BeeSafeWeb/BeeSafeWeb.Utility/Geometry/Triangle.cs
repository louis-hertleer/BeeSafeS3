using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeeSafeWeb.Utility.Geometry;

public class Triangle
{
    public Point[] Points { get; private set; }

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

        points[0] = rays[0].Intersect(rays[1]) ?? throw new Exception("Ray 1 never intersects with Ray 2");
        points[1] = rays[1].Intersect(rays[2]) ?? throw new Exception("Ray 2 never intersects with Ray 3");
        points[2] = rays[2].Intersect(rays[0]) ?? throw new Exception("Ray 3 never intersects with Ray 1");

        Triangle triangle = new Triangle(points);

        /* find the longest path to the center. */
        double length = triangle.Points.Select(p => p.Distance(triangle.Center)).Max();

        return new Circle(length, triangle.Center);
    }
}