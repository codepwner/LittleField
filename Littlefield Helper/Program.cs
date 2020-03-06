using ConsoleTableExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using static Fetch;
using static Utilities;

namespace Littlefield_Helper
{
    class Program
    {
        private static Timer updateTimer;

        public static Line Line = new Line(
            new List<Machine>
            {
                new Machine(1, Stations.S1Q, Stations.S1UTIL, new List<Process> { new Process(1) }),
                new Machine(2, Stations.S2Q, Stations.S2UTIL, new List<Process> { new Process(2), new Process(4) }),
                new Machine(3, Stations.S3Q, Stations.S3UTIL, new List<Process> { new Process(3) }),
            });

        static void Main(string[] args)
        {
            Data.Serialize();
            updateTimer = new Timer(30 * 1000)
            {
                AutoReset = true,
                Enabled = true
            };
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            UpdateTimer_Elapsed(null, null);

            Console.ReadLine();
            updateTimer.Stop();
            updateTimer.Dispose();
        }

        private static void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Data.Refresh();
            var demand = Demand.EstimateDemand(Situation.ROLLING_WINDOW);

            var place = Data.Standings.First(a => a.Name.Equals("mbawesome")).Place;

            Console.Clear();
            Console.WriteLine($"Updated: {DateTime.Now}");
            Console.WriteLine($"Day: {Data.Base["[Status] Day"]}");
            Console.WriteLine($"Place: {place}");
            Console.WriteLine($"Bank:           {AsInt(Data.Base["[Status] Cash Balance"]),14:C0}");
            if (place > 1)
                Console.WriteLine($"  >> Behind By: {Data.Standings.First(a => a.Place == 1).Money - Data.Standings.First(a => a.Place == place).Money,14:C0}");
            else
                Console.WriteLine($"  >> Ahead By:  {Data.Standings.First(a => a.Place == 1).Money - Data.Standings.First(a => a.Place == 2).Money,14:C0}");

            Console.WriteLine("\n===========THROUGHPUT===========");
            var realAvgThroughput = Data.Queues[Stations.JOBOUT].TakeLast(Situation.ROLLING_WINDOW).Average(i => i.Value);
            Console.WriteLine($"  >> Average Real Throughput (averaged over 4 days):  {Math.Round(realAvgThroughput) * Situation.JOB_SIZE,8:F0} ({Math.Round(realAvgThroughput):F0} jobs)");
            Console.WriteLine($"  >> Projected Demand ({Situation.ROLLING_WINDOW} days ahead):                {Math.Round((double)demand),8} ({Math.Round((double)demand / Situation.JOB_SIZE):F0} jobs)");

            Console.WriteLine("\n===========INVENTORY===========");
            Console.WriteLine(PInventory.Current.ToString());

            Console.WriteLine($"\n===========STATIONS===========");
            var stationInfo = new List<dynamic> 
            {
                new {
                    Station = $"LINE",
                    EstCapacity = $"{Line.Capacity(Situation.Current) / Situation.JOB_SIZE,8:F1}",
                    ActThrPut = $"{Data.Queues[Stations.JOBOUT].Last().Value,8:F2}",
                    EstFlowTime = $"{Line.FlowTime(Situation.Current),8:F2}",
                    ActFlowTime = $"{Data.Queues[Stations.JOBT].TakeLast(Situation.ROLLING_WINDOW).Average(v => v.Value),8:F2}",
                    Util = $"",
                    EstQueueCnt = $"",
                    ActQueueCnt = $"{Data.Queues[Stations.JOBQ].TakeLast(Situation.ROLLING_WINDOW).Average(q => q.Value),8:F1}",
                    EstQueueTime = $"{24 * Line.QueueTime(Situation.Current, demand),8:F2}",
                    ActQueueTime = $"",
                } 
            };
            foreach (var machine in Line.Machines)
            {
                var recentQueueLength = Data.Queues[machine.Queue].TakeLast(Situation.ROLLING_WINDOW).Average(q => q.Value) * Situation.JOB_SIZE;

                stationInfo.Add(new
                {
                    Station = $"S{machine.ID}",
                    EstCapacity = $"{machine.Capacity(Situation.Current) / Situation.JOB_SIZE,8:F1}",
                    ActThrPut = $"",
                    EstFlowTime = $"{machine.FlowTime(Situation.Current),8:F2}",
                    ActFlowTime = $"",
                    Util = $"{Data.Queues[machine.Utilization].Values.TakeLast(Situation.ROLLING_WINDOW).Average(),4:P0}",
                    EstQueueCnt = $"{machine.QueueLength(Situation.Current, Line, demand),8:F1}",
                    ActQueueCnt = $"{Data.Queues[machine.Queue].TakeLast(Situation.ROLLING_WINDOW).Average(q => q.Value),8:F1}",
                    EstQueueTime = $"{machine.QueueTime(Situation.Current, Line, demand),8:F2}",
                    ActQueueTime = $"{recentQueueLength / machine.Capacity(Situation.Current),8:F2}",
                });
            }
            stationInfo.Add(new
            {
                Station = $"",
                EstCapacity = $"(jobs/day)",
                ActThrPut = $"(jobs/day)",
                EstFlowTime = $"(days)",
                ActFlowTime = $"(days)",
                Util = $"",
                EstQueueCnt = $"(jobs)",
                ActQueueCnt = $"(jobs)",
                EstQueueTime = $"(days)",
                ActQueueTime = $"(days)",
            });
            ConsoleTableBuilder.From(stationInfo).WithFormat(ConsoleTableBuilderFormat.MarkDown).ExportAndWrite();
            Console.WriteLine($"\n===========FACTORY SETUPS===========");
            Situation.FindOptimalSetup(Line, demand);

            //Console.WriteLine($"\n===========AUTO-CHECKS===========");
            //if ()
        }
    }
}
