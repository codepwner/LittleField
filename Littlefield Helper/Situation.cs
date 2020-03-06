using ConsoleTableExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Utilities;

public class Situation
{
    public const int JOB_SIZE = 60;
    public const int ROLLING_WINDOW = 10;

    public Dictionary<int, int> MachineCount { get; set; }
    public int Splits { get; set; }
    public Contract Contract { get; set; }
    public double Throughput => Data.Queues[Fetch.Stations.JOBOUT].TakeLast(ROLLING_WINDOW).Average(i => i.Value);
    public double CurrentLeadTime => Data.Queues[Fetch.Stations.JOBT].TakeLast(ROLLING_WINDOW).Average(i => i.Value);
    public int CurrentDay => AsInt(Data.Base["[Status] Day"]);
    public static Dictionary<int, int> CurrentMachineCounts => new Dictionary<int, int>
                {
                    { 1, AsInt(Data.Base["[S1] Number of Installed Machines"]) },
                    { 2, AsInt(Data.Base["[S2] Number of Installed Machines"]) },
                    { 3, AsInt(Data.Base["[S3] Number of Installed Machines"]) },
                };

    public static Situation Current
    {
        get
        {
            return new Situation
            {
                Splits = AsInt(Extract(Data.Base["[Orders] Lot size"], ", or ", " lots")),
                Contract = Data.Contracts[AsInt(Data.Base["[Orders] Current contract"])],
                MachineCount = CurrentMachineCounts
            };
        }
    }

    public double TotalRevenue(Line line)
    {
        var currentDay = AsInt(Data.Base["[Status] Day"]);
        var days = Enumerable.Range(1, Case.LastDay - currentDay);
        var demand = days.ToDictionary(d => d, d => Demand.EstimateDemand(d));
        var throughput = demand.ToDictionary(d => d.Key, d => Math.Min(d.Value, 24 * line.Capacity(this)));
        var flowTime = demand.ToDictionary(d => d.Key, d => line.FlowTime(this));
        var queueTime = demand.ToDictionary(d => d.Key, d => line.QueueTime(this, d.Value));
        var revenue = days.ToDictionary(d => d, d => throughput[d] / JOB_SIZE * Contract.RevenuePerJob(queueTime[d] + flowTime[d]));

        return revenue.Sum(d => d.Value);
    }

    public static void FindOptimalSetup(Line line, double demand)
    {
        var situations = new List<Situation>();
        foreach (var split in Case.LotSplitsAvailable)
        {
            foreach (var contract in Data.Contracts.Values)
            {
                situations.Add(new Situation
                {
                    Contract = contract,
                    Splits = split,
                    MachineCount = CurrentMachineCounts
                });
                situations.Add(new Situation
                {
                    Contract = contract,
                    Splits = split,
                    MachineCount = new Dictionary<int, int> { 
                        { 1,1 },
                        { 2,1 },
                        { 3,1 }
                    }
                });
            }
        }
        foreach(var machine in line.Machines)
        {
            var sit = new Situation
            {
                Contract = Current.Contract,
                Splits = Current.Splits,
                MachineCount = CurrentMachineCounts
            };

            sit.MachineCount[machine.ID]++;

            situations.Add(sit);
        }

        var items = new List<dynamic>();
        
        var current = Current;
        foreach (var sit in situations.Append(current))
        {
            while (line.Capacity(sit) < demand)
            {
                var bottleneck = line.Machines.OrderBy(m => m.Capacity(sit)).First();
                sit.MachineCount[bottleneck.ID]++;
            }
            var costUpgrades = sit.CostForUpgrade(line);
            var flowTime = line.FlowTime(sit);
            var queueTime = line.QueueTime(sit, demand);
            var revPerJob = sit.Contract.RevenuePerJob(flowTime + queueTime);
            var jobThroughput = demand / JOB_SIZE;
            var revenue = revPerJob * jobThroughput * (Case.LastDay - Situation.Current.CurrentDay);

            items.Add(new
            {
                Note = current.Equals(sit) ? "CURRENT >> " : "",
                Splits = sit.Splits,
                Contract = sit.Contract.ID,
                S1 = sit.MachineCount[1],
                S2 = sit.MachineCount[2],
                S3 = sit.MachineCount[3],
                UpgradeCost = $"{costUpgrades:C0}",
                FlowTime = $"{flowTime:F2}",
                QueueTime = $"{queueTime:F2}",
                LeadTime = $"{flowTime + queueTime:F2}",
                JobRevenue = $"{revPerJob:C0}",
                Throughput = $"{jobThroughput:F1}",
                ResultEnd = $"{(revPerJob * jobThroughput * (Case.LastDay - Current.CurrentDay)) - costUpgrades,12:C0}",
            });
        }

        ConsoleTableBuilder.From(items.OrderByDescending(i => int.Parse(i.ResultEnd, System.Globalization.NumberStyles.Any)).ToList()).WithFormat(ConsoleTableBuilderFormat.MarkDown).ExportAndWriteLine();
    }

    public double CostForUpgrade(Line line)
    {
        var cost = 0;
        foreach (var machine in MachineCount)
        {
            var delta = Math.Max(0, machine.Value - CurrentMachineCounts[machine.Key]);
            cost += delta * line.Machines[machine.Key - 1].Cost;
        }
        foreach (var machine in MachineCount)
        {
            var delta = Math.Max(0, CurrentMachineCounts[machine.Key] - machine.Value);
            cost -= delta * line.Machines[machine.Key - 1].Salvage;
        }
        return cost;
    }
}
