using BeeSafeWeb.Utility.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeeSafeWeb.Tests;

[TestClass]
public class DeviceTests
{
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