using BeeSafeWeb.Utility.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeeSafeWeb.Tests;

[TestClass]
public class UtilityTests
{
    [DataRow("1 day ago", 1)]
    [DataRow("5 days ago", 5)]
    [DataTestMethod]
    public void TestWhetherLastActiveStringReturnsADayAgo(string expected, int dayCount)
    {
        var lastActive =
            DateUtility.GetLastActiveString(
                DateTime.Now.Subtract(TimeSpan.FromDays(dayCount)));

        Assert.AreEqual(expected, lastActive);
    }

    [DataRow("Just now", 10)]
    [DataRow("39 seconds ago", 39)]
    [DataTestMethod]
    public void TestWhetherLastActiveStringReturnsJustNow(string expected, int secondCount)
    {
        var lastActive =
            DateUtility.GetLastActiveString(
                DateTime.Now.Subtract(TimeSpan.FromSeconds(secondCount)));

        Assert.AreEqual(expected, lastActive);
    }

    [DataRow("1 minute ago", 1)]
    [DataRow("10 minutes ago", 10)]
    [DataTestMethod]
    public void TestWhetherLastActiveStringReturnsMinuteAgo(string expected, int minuteCount)
    {
        var lastActive =
            DateUtility.GetLastActiveString(
                DateTime.Now.Subtract(TimeSpan.FromMinutes(minuteCount)));

        Assert.AreEqual(expected, lastActive);
    }
}