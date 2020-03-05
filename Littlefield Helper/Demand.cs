using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Situation;

public class Demand
{
    public const int WINDOW = 25;

    public static double Trend
    {
        get
        {
            var a = Data.Demand.TakeLast(WINDOW).Average(a => a.Value) * JOB_SIZE;
            var b = Data.Demand.SkipLast(1).TakeLast(WINDOW).Average(a => a.Value) * JOB_SIZE;

            return (a - b) / WINDOW;
        }
    }
    public static int EstimateDemand(int daysAhead, double? trend = null)
    {
        trend ??= Trend;
        return (int)Math.Round((Data.Demand.TakeLast(WINDOW).Average(a => a.Value) + trend.Value * (daysAhead /*+ (WINDOW / 2)*/)) * JOB_SIZE);
    }

    public static double DemandVariance()
    {
        double mean = EstimateDemand(0);
        double variance = 0;

        for (int i = 0; i < WINDOW; i++)
        {
            variance += Math.Pow(((Data.Demand[Situation.Current.CurrentDay - i - 1] * Situation.JOB_SIZE) - mean), 2);
        }        
        return 2 * Math.Sqrt(variance / WINDOW);
    }
}
