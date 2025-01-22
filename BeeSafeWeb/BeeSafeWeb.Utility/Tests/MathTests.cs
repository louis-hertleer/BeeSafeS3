using Microsoft.VisualStudio.TestTools.UnitTesting;
using BeeSafeWeb.Utility.Geometry;

namespace BeeSafeWeb.Tests;

[TestClass]
public class MathTests
{
    [DataRow( new double[] { 0, 0, 180 }, new double[]{ 5, 0, 0 }, true)]
    [DataRow( new double[] { 0, 0, 180 }, new double[]{ 5, 0, 180 }, true)]
    [DataRow( new double[] { 0, 0, 180 }, new double[]{ 5, 0, 51 }, false)]
    [DataRow( new double[] { 0, 0, 22 }, new double[]{ 5, 0, -45 }, false)]
    [DataTestMethod]
    public void TestWhether180DegreeRaysAreParallel(double[] d1, double[] d2, bool isParallel)
    {
        Ray r1 = new(d1[0], d1[1], d1[2]);
        Ray r2 = new(d1[0], d1[1], d1[2]);

        Assert.IsTrue(r1.IsParallelWith(r2));
    }

    [DataRow(new double[] { 0, 0, 10 }, new double[] { 5, 0, -45 }, new double[] { 0.7494, 4.2505})]
    [DataRow(new double[] { 0, 0, 0 }, new double[] { 2, 0, -45 }, new double[] { 0.0, 2.0})]
    [DataRow(new double[] { 0, 0, 0 }, new double[] { 10, 0, 0 }, null)]
    [DataTestMethod]
    public void TestWhetherLinesIntersect(double[] d1, double[] d2,
        double[]? intersection)
    {
        Ray r1 = new(d1[0], d1[1], d1[2]);
        Ray r2 = new(d2[0], d2[1], d2[2]);

        Point? point = r1.Intersect(r2);

        if (intersection is not null)
        {
            Assert.IsNotNull(point, "The rays should intersect.");
            Assert.AreEqual(intersection[0], point.X, 1e-4, "X coordinate does not match.");
            Assert.AreEqual(intersection[1], point.Y, 1e-4, "Y coordinate does not match.");
        }
        else
        {
            Assert.IsNull(point);
        }
    }

    [DataRow(new double[] { 0, 0 }, new double[] { 0, 5 }, 5)]
    [DataRow(new double[] { -2.8, 0 }, new double[] { -3, 1 }, 1.019804)]
    [DataRow(new double[] { 0, 0}, new double[] { 1, 1 }, 1.4142)]
    [DataTestMethod]
    public void TestWhetherDistanceIsCorrect(double[] d1, double[] d2,
        double distance)
    {
        Point p1 = new(d1[0], d1[1]);
        Point p2 = new(d2[0], d2[1]);
        
        Assert.AreEqual(distance, 1e-4, p1.Distance(p2));
    }

    [DataRow(
        new double[] { 0, 0, 57.4906 }, new double[] { 5, 0, -37.86 }, new double[] { 2, 6, 170.5377 },
        new double[] {3.3435, 2.1308}, new double[] {2.45412, 3.27488}, new double[] {2.71195, 1.72832}, 
        new double[] {2.83656, 2.37801, 0.97498}
    )]
    [DataTestMethod]
    public void TestWhetherTriangulationIsCorrect(double[] dRay1,
        double[] dRay2, double[] dRay3, double[] dPoint1, double[] dPoint2,
        double[] dPoint3, double[] dCenter)
    {
        Ray r1 = new(dRay1[0], dRay1[1], dRay1[2]);
        Ray r2 = new(dRay2[0], dRay2[1], dRay2[2]);
        Ray r3 = new(dRay3[0], dRay3[1], dRay3[2]);

        Circle? circle = Triangle.Triangulate([r1, r2, r3]);

        Assert.IsNotNull(circle);

        Assert.AreEqual(dCenter[0], circle.Center.X, 0.0001);
        Assert.AreEqual(dCenter[1], circle.Center.Y, 0.0001);
        Assert.AreEqual(dCenter[2], circle.Radius, 0.0001);
    }
}