namespace BeeSafeWeb.Utility.Geometry;

public class Ray
{
    private double _direction;

    public Point Point { get; set; }

    public double Direction
    {
        get => _direction;
        set => _direction = value % 360;
    }

    public Ray()
    {
        Point = new Point();
        Direction = 0;
    }

    public Ray(double x, double y, double direction)
    {
        Point = new Point(x, y);
        Direction = direction;
    }

    /// <summary>
    /// Finds whether this ray is parallel and thus never intersect.
    /// </summary>
    /// <param name="ray"></param>
    /// <returns>Whether the ray is parallel with this ray.</returns>
    public bool IsParallelWith(Ray ray)
    {
        const double Tolerance = 1e-6; // A small tolerance value for floating-point comparison

        // Normalize directions to the range [0, 360)
        double thisDir = this.Direction % 360;
        double otherDir = ray.Direction % 360;

        // Check if the directions are the same or opposite
        return (System.Math.Abs(thisDir - otherDir) < Tolerance
                || System.Math.Abs((thisDir + 180) % 360 - otherDir) < Tolerance);
    }

    /// <summary>
    /// Finds the intersection point of two rays.
    /// </summary>
    /// <param name="ray"></param>
    /// <returns>The intersection point, or null if they never intersect.</returns>
    public Point? Intersect(Ray ray)
    {
        if (IsParallelWith(ray))
        {
            return null; // Parallel rays do not intersect
        }

        // Extract components for this ray
        double x1 = this.Point.X;
        double y1 = this.Point.Y;
        double theta1 = this.Direction * System.Math.PI / 180.0; // Convert to radians
        double dx1 = System.Math.Sin(theta1);
        double dy1 = System.Math.Cos(theta1);

        // Extract components for the other ray
        double x2 = ray.Point.X;
        double y2 = ray.Point.Y;
        double theta2 = ray.Direction * System.Math.PI / 180.0; // Convert to radians
        double dx2 = System.Math.Sin(theta2);
        double dy2 = System.Math.Cos(theta2);

        // Solve the linear system:
        double determinant = dx1 * dy2 - dy1 * dx2;

        if (System.Math.Abs(determinant) < 1e-6)
        {
            return null; // Shouldn't reach here if IsParallelWith is correct
        }

        double t1 = ((x2 - x1) * dy2 - (y2 - y1) * dx2) / determinant;

        // Compute the intersection point
        double ix = x1 + t1 * dx1;
        double iy = y1 + t1 * dy1;

        return new Point(ix, iy);
    }
}