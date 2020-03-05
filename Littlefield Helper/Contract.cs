using System;
using System.Collections.Generic;
using System.Text;
using static Utilities;
using System.Linq;

public class Contract
{
    public Contract(int id)
    {
        ID = id;
    }
    public int ID { get; private set; }
    public double PromiseTimeDays => AsDouble(Data.Base[$"[Contract {ID}] Quoted Lead Time"]);
    public double MaximumTimeDays => AsDouble(Data.Base[$"[Contract {ID}] Maximum Lead Time"]);
    public double PromiseRevenue => AsDouble(Data.Base[$"[Contract {ID}] Revenue / Order"]);

    public static Dictionary<int, Contract> All => 
        Data.Base.Keys.Where(k => k.StartsWith("[Contract "))
        .GroupBy(c => int.Parse(c.Substring(10,1)))
        .ToDictionary(c => c.Key, c => new Contract(c.Key));

    public double RevenuePerJob(double deliveryTime)
    {
        if (deliveryTime < PromiseTimeDays)
            return PromiseRevenue;
        else if (deliveryTime > MaximumTimeDays)
            return 0;
        else
            return PromiseRevenue * (1 - ((deliveryTime - PromiseTimeDays) / (MaximumTimeDays - PromiseTimeDays)));
    }
}
