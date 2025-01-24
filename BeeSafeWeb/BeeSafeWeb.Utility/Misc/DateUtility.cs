namespace BeeSafeWeb.Utility.Misc;

public class DateUtility
{
    public static string GetLastActiveString(DateTime dateTime)
    {
        var date = DateTime.Now - dateTime;
        int number = 0;
        string unit = "Just now";
        if (date.Days > 0)
        {
            number = date.Days;
            unit = "day";
        }
        else if (date.Hours > 0)
        {
            number = date.Hours;
            unit = "hour";
        }
        else if (date.Minutes > 0)
        {
            number = date.Minutes;
            unit = "minute";
        }
        else if (date.Seconds > 30)
        {
            number = date.Seconds;
            unit = "second";
        }
        else
        {
            return unit;
        }

        if (number != 1)
        {
            unit += "s";
        }

        return $"{number} {unit} ago";
    }
}