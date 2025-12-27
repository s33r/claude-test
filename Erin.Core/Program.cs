namespace Erin.Core;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("ISODateIndex Demo");
        Console.WriteLine("=================\n");

        // Create from components
        var isoDate1 = new ISODateIndex(2025, 1, 0); // 2025, Week 1, Monday
        Console.WriteLine($"ISO Date 1: {isoDate1}");
        Console.WriteLine($"  -> DateTime: {isoDate1.ToDateTime():yyyy-MM-dd}\n");

        // Create from DateTime
        var today = DateTime.Today;
        var isoDate2 = ISODateIndex.FromDateTime(today);
        Console.WriteLine($"Today: {today:yyyy-MM-dd}");
        Console.WriteLine($"  -> ISO Date: {isoDate2}\n");

        // Demonstrate date arithmetic
        var nextWeek = isoDate2.AddWeeks(1);
        Console.WriteLine($"Next week: {nextWeek}");
        Console.WriteLine($"  -> DateTime: {nextWeek.ToDateTime():yyyy-MM-dd}\n");

        // Show weeks in year
        Console.WriteLine($"Weeks in 2025: {ISODateIndex.GetWeeksInYear(2025)}");
        Console.WriteLine($"Weeks in 2024: {ISODateIndex.GetWeeksInYear(2024)}");
    }
}
