using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Fetch;
using static Utilities;

public class Machine
{
    public Machine (int id, Stations queue, Stations utilization, List<Process> processes)
    {
        ID = id;
        Processes = processes;
        Queue = queue;
        Utilization = utilization;
    }
    public int ID { get; private set; }
    public int Cost => AsInt(Extract(Data.Base[$"[S{ID}] Purchase Price"], "$ "));
    public int Salvage => AsInt(Extract(Data.Base[$"[S{ID}] Retirement Price"], "$ "));
    public int LeadTime => AsInt(Data.Base[$"[S{ID}] Purchase lead time"]);
    public List<Process> Processes { get; private set; }
    public Stations Queue { get; set; }
    public Stations Utilization { get; set; }

    // in days
    public double FlowTime(Situation situation)
    {
        return Processes.Sum(s => s.FlowTime(situation));
    }

    // in days
    public double QueueTime(Situation situation, Line line, double demand)
    {
        //return QueueLength(situation, line, demand) * FlowTime(situation);
        var arrivalRate = DownstreamArrival(situation, line, demand);
        var serviceRate = Capacity(situation);
        return (1 / (serviceRate - arrivalRate)) - (1 / serviceRate);
    }

    public double QueueLength(Situation situation, Line line, double demand)
    {
        //var waveQty = line.Machines.Where(n => n.ID < ID).Select(n => n.Capacity(situation)).Append(demand).Min();
        //var turns = (waveQty / (Situation.JOB_SIZE / situation.Splits)) / situation.MachineCount[ID];

        //return turns / 2;

        //var serviceRate = Capacity(situation);
        //return (1 / (serviceRate - arrivalRate)) - (1/serviceRate);

        var arrivalRate = DownstreamArrival(situation, line, demand);

        return QueueTime(situation, line, demand) * arrivalRate;
    }

    // in units per day
    public double Capacity(Situation situation)
    {
        return (1 / FlowTime(situation)) * situation.MachineCount[ID] * (Situation.JOB_SIZE / situation.Splits);
    }

    public double DownstreamArrival(Situation situation, Line line, double demand)
    {
        if(ID == 1)
        {
            return demand;
        }
        else
        {
            return Math.Min(demand, line.Machines.Where(n => n.ID < ID).Select(n => n.Capacity(situation)).Min());
        }
    }
}