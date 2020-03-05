using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Utilities;

public class Inventory
{
    public int CurrentPosition => (int)Data.Queues[Fetch.Stations.INV].Last().Value;
    public int OrderUpToLevel => AsInt(Extract(Data.Base["[Materials] Order-Up-To Level"], "", " kits"));
    public double UnitCost => AsDouble(Extract(Data.Base["[Materials] Unit Cost"], "$ "));
    public double OrderCost => AsDouble(Extract(Data.Base["[Materials] Order Cost"], "$ "));
    public int LeadTime => AsInt(Extract(Data.Base["[Materials] Lead Time"], "", " day(s)"));
    public virtual int SafetyStock => (int)(Demand.DemandVariance() * LeadTime);
    public int OptimalOrderQuantity => (int)Math.Round(Math.Sqrt((2 * 365 * Demand.EstimateDemand(0) * OrderCost) / (UnitCost * Case.CostOfCapital)));
    public double EffectiveUnitPrice(int orderAmount) => ((UnitCost * orderAmount) + OrderCost) / orderAmount;

    public override string ToString()
    {
        return $"  >> Recommended Safety Stock: {SafetyStock:F0} kits\n"
            + $"  >> Optimal Order Quantity: {OptimalOrderQuantity:F0} kits [Currently: {OrderUpToLevel:F0}]\n"
            + $"  >> Optimal Effective Unit Cost: {EffectiveUnitPrice(OptimalOrderQuantity):C2} [Currently: {EffectiveUnitPrice(OrderUpToLevel):C2}]\n";
    }
}
