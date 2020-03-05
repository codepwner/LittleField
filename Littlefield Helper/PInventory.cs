using System;
using System.Collections.Generic;
using System.Text;
using static Utilities;


public class PInventory : Inventory
{
    public int Interval => AsInt(Extract(Data.Base["[Materials] Review Period"], "", " day(s)"));
    public override int SafetyStock => (int)(Demand.DemandVariance() * (Interval + LeadTime));

    public static PInventory Current => new PInventory { };

    //public double AverageUnitCost()
    //{
    //    return 1;
    //}

    public int OptimalInterval => (int)Math.Round((double)OptimalOrderQuantity / Demand.EstimateDemand(0));

    public int OptimalOrderUpToLevel => SafetyStock + Demand.EstimateDemand((Interval + LeadTime) / 2) * OptimalInterval;

    public override string ToString()
    {
        return base.ToString()
            + $"  >> Optimal Interval: {OptimalInterval} [Currently: {Interval:F0}]\n"
            + $"  >> Reorder Up To: {OptimalOrderUpToLevel} [Currently: {OrderUpToLevel:F0}]";
    }
}
