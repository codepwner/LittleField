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
                new Machine(1, new List<Process> { new Process(1) }),
                new Machine(2, new List<Process> { new Process(2), new Process(4) }),
                new Machine(3, new List<Process> { new Process(3) }),
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
            Data.Reset();
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
            Console.WriteLine($"  >> Projected Demand ({Situation.ROLLING_WINDOW} days ahead):                {Math.Round((double)demand),8} ({Math.Round((double) demand / Situation.JOB_SIZE):F0} jobs)");

            Console.WriteLine("\n===========INVENTORY OPTIMIZATION===========");
            Console.WriteLine(PInventory.Current.ToString());

            Console.WriteLine($"\n===========STATIONS===========");
            var stationInfo = new List<dynamic>();
            foreach (var machine in Line.Machines)
            {
                stationInfo.Add(new
                {
                    Station = $"S{machine.ID}",
                    Capacity = $"{machine.Capacity(Situation.Current) / Situation.JOB_SIZE,8:F1}",
                    CycleTime = $"{24 * machine.FlowTime(Situation.Current),8:F2}"
                });
            }
            stationInfo.Add(new
            {
                Station = $"LINE (Estimated)",
                Capacity = $"{Line.Capacity(Situation.Current) / Situation.JOB_SIZE,8:F1}",
                CycleTime = $"{24 * Line.FlowTime(Situation.Current),8:F2}"
            });
            stationInfo.Add(new
            {
                Station = $"LINE (Actual)",
                Capacity = $"{Data.Queues[Stations.JOBOUT].Last().Value,8:F2}",
                CycleTime = $"{24 * Data.Queues[Stations.JOBT].TakeLast(Situation.ROLLING_WINDOW).Average(v => v.Value),8:F2}"
            });
            ConsoleTableBuilder.From(stationInfo).WithFormat(ConsoleTableBuilderFormat.MarkDown).ExportAndWrite();

            //Console.WriteLine($"\nCapacity:       {Line.Capacity(Situation.Current) / Situation.JOB_SIZE,8:F1} (jobs/day) [Latest: {Data.Queues[Stations.JOBOUT].Last().Value:F2}]");
            //foreach (var machine in Line.Machines)
            //    Console.WriteLine($"  >> {machine.ID}:        {machine.Capacity(Situation.Current) / Situation.JOB_SIZE,8:F1}");

            //Console.WriteLine($"\nWork Flow:     {24 * Line.FlowTime(Situation.Current),8:F2} (hours) [Latest: {24 * Data.Queues[Stations.JOBT].Last().Value:F2}]");
            //foreach (var machine in Line.Machines)
            //{
            //    Console.WriteLine($"  >> {machine.ID}:        {24 * machine.FlowTime(Situation.Current),8:F2}");
            //}

            var queueInfo = new List<dynamic>();
            var queues = new Dictionary<int, Stations>
            {
                {0, Stations.S1Q },
                {1, Stations.S2Q },
                {2, Stations.S3Q }
            };

            Console.WriteLine($"\n===========QUEUES===========");
            queueInfo.Add(new
            {
                Station = "JOBQ",
                CalcQueued = "",
                ActualQueued = $"{Data.Queues[Stations.JOBQ].TakeLast(Situation.ROLLING_WINDOW).Average(q => q.Value)}",
                CalcQueueTime = $"",
                ActualQueueTime = "",
            });
            foreach (var queue in queues)
            {
                var machine = Line.Machines[queue.Key];
                var recentQueueLength = Data.Queues[queue.Value].TakeLast(Situation.ROLLING_WINDOW).Average(q => q.Value) * Situation.JOB_SIZE;               

                queueInfo.Add(new
                {
                    Station = $"S{machine.ID}Q",
                    CalcQueued = $"{Line.Machines[queue.Key].QueueLength(Situation.Current, Line, demand):F1}",
                    ActualQueued = $"{Data.Queues[queue.Value].TakeLast(Situation.ROLLING_WINDOW).Average(q => q.Value):F1}",
                    CalcQueueTime = $"{24 * machine.QueueTime(Situation.Current, Line, demand),8:F2}",
                    ActualQueueTime = $"{recentQueueLength / machine.Capacity(Situation.Current):F2}"
                });
            }

            queueInfo.Add(new
            {
                Station = "LINE",
                CalcQueued = $"{Data.Queues[Stations.JOBQ].TakeLast(Situation.ROLLING_WINDOW).Average(q => q.Value):F1}",
                ActualQueued = "",
                CalcQueueTime = $"{24 * Line.QueueTime(Situation.Current, demand),8:F2}",
                ActualQueueTime = "",
            });          

            ConsoleTableBuilder.From(queueInfo).WithFormat(ConsoleTableBuilderFormat.MarkDown).ExportAndWrite();         

            Console.WriteLine($"\n===========FACTORY SETUPS===========");
            Situation.FindOptimalSetup(Line, demand);
        }
    }
}
