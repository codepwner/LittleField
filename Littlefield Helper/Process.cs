using System;
using System.Collections.Generic;
using System.Text;

public class Process
{
    public Process(int step)
    {
        Step = step;
    }
    public int Step { get; private set; }
    public double SetupTime => Case.SetupTimeByStep[Step] / 24;
    public double UnitTime => Case.UnitTimeByStep[Step] / 24;

    public double FlowTime(Situation situation)
    {
        return SetupTime + (UnitTime * Situation.JOB_SIZE / situation.Splits);
    }
}
