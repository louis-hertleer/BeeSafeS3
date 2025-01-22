using BeeSafeWeb.Utility.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeeSafeWeb.Tests;

[TestClass]
public class DeviceTests
{
    [TestMethod]
    public void TestWhetherLastActiveStringReturnsADayAgo()
    {
        Device device = new()
        {
            LastActive = DateTime.Now.Subtract(TimeSpan.FromDays(1)),
        };

        Assert.AreEqual("1 day ago", device.LastActiveString);
    }

    [TestMethod]
    public void TestWhetherLastActiveStringReturnsDaysAgo()
    {
        Device device = new()
        {
            LastActive = DateTime.Now.Subtract(TimeSpan.FromDays(5)),
        };

        Assert.AreEqual("5 days ago", device.LastActiveString);
    }

    [TestMethod]
    public void TestWhetherLastActiveStringReturnsJustNow()
    {
        Device device = new()
        {
            LastActive = DateTime.Now.Subtract(TimeSpan.FromSeconds(5)),
        };

        Assert.AreEqual("Just now", device.LastActiveString);
    }

    [TestMethod]
    public void TestWhetherLastActiveStringReturnsMinuteAgo()
    {
        Device device = new()
        {
            LastActive = DateTime.Now.Subtract(TimeSpan.FromMinutes(1)),
        };

        Assert.AreEqual("1 minute ago", device.LastActiveString);
    }

    [TestMethod]
    public void TestWhetherTenMinutesAgoIsOffline()
    {
        Device device = new()
        {
            LastActive = DateTime.Now.Subtract(TimeSpan.FromMinutes(10)),
        };

        Assert.IsFalse(device.IsOnline);
    }

    [TestMethod]
    public void TestWhetherOneMinuteAgoIsOnline()
    {
        Device device = new()
        {
            LastActive = DateTime.Now.Subtract(TimeSpan.FromMinutes(1)),
        };

        Assert.IsTrue(device.IsOnline);
    }
}