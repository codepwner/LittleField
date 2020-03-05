using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class Line
{
    public Line (List<Machine> machines)
    {
        Machines = machines;
    }
    public List<Machine> Machines { get; private set; }
    public double FlowTime(Situation situation)
    {
        return Machines.Sum(s => s.FlowTime(situation));
    }

    public double QueueTime(Situation situation, double demand)
    {
        return Machines.Sum(m => m.QueueTime(situation, this, demand));
    }

    public double Capacity(Situation situation)
    {
        return Machines.Min(s => s.Capacity(situation));
    }
}