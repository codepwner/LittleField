using System;
using System.Collections.Generic;
using System.Text;

static class Case
{
    // in hours
    public static Dictionary<int, double> SetupTimeByStep = new Dictionary<int, double>
    {
        {1, 1.5 },
        {2, 0.25 },
        {3, 1 },
        {4, 0.25 }
    };
    public static Dictionary<int, double> UnitTimeByStep = new Dictionary<int, double>
    {
        {1, .01 },
        {2, .25 },
        {3, .001 },
        {4, 0.05 }
    };
    public static int LastDay = 600;
    public static double CostOfCapital = .14;
    public static List<int> LotSplitsAvailable = new List<int> { 1, 2, 3, 5, 10, 12 };
}
