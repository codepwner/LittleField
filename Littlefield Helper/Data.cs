using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using static Fetch;

public static class Data
{
    public static void Refresh()
    {
        _demand = GetPlot(Fetch.Stations.JOBIN);
        _base = GetBase();
        _standings = Position.Current;
        _queues = GetQueues();
        //_contracts = null;
    }

    private static Dictionary<int, double> _demand;
    public static Dictionary<int, double> Demand => _demand ?? (_demand = GetPlot(Fetch.Stations.JOBIN));

    private static Dictionary<string, string> _base;
    public static Dictionary<string, string> Base => _base ?? (_base = GetBase());

    private static IEnumerable<Position> _standings;
    public static IEnumerable<Position> Standings => _standings ?? (_standings = Position.Current);

    private static Dictionary<Stations, Dictionary<int, double>> _queues;
    public static Dictionary<Stations, Dictionary<int, double>> Queues => _queues ?? (_queues = GetQueues());

    private static Dictionary<int, Contract> _contracts;
    public static Dictionary<int, Contract> Contracts => _contracts ?? (_contracts = Contract.All);

    public static void Serialize()
    {
        var data = JsonConvert.SerializeObject(new { Demand, Base, Standings, Queues, Contracts }, Formatting.Indented);
        using (var writer = new StreamWriter("snapshot.json"))
        {
            writer.Write(data);
        }
    }
}
