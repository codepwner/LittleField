using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using static Utilities;

public static class Fetch
{
    static Fetch()
    {
        Authenticate();
    }

    private static CookieContainer Cookies = new CookieContainer();
    private static HttpClientHandler Handler = new HttpClientHandler() { CookieContainer = Cookies, AllowAutoRedirect = true, UseCookies = true };
    public enum Stations { JOBIN, JOBQ, INV, S1Q, S1UTIL, S2Q, S2UTIL, S3Q, S3UTIL, JOBOUT, JOBT, JOBREV }
    public enum Stats { AVG }

    private static HttpClient Client = new HttpClient(Handler)
    {
        BaseAddress = new Uri("http://op.responsive.net/")
    };

    public static Dictionary<int, double> GetPlot(Stations station)
    {
        var data = RequestData($"/Littlefield/Plot?data={Enum.GetName(typeof(Stations), station)}&x=all");

        var rawPoints = Extract(data, "{label: 'data', points: '", "'}\n");
        var allValues = rawPoints.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        return allValues.Select((v, i) => new { index = i, Value = double.Parse(v) }).Where(i => i.index % 2 == 1).ToDictionary(i => i.index / 2, i => i.Value);
    }

    public static string RequestData(string path)
    {
        var request = Client.GetAsync(path).Result;

        return request.Content.ReadAsStringAsync().Result;
    }

    public static void Authenticate()
    {
        Client.DefaultRequestHeaders.Add("Host", "op.responsive.net");

        var values = new Dictionary<string, string>
            {
                { "id", "mbawesome" },
                { "password", "maroon2" },
                { "institution", "collins" }
            };
        var content = new FormUrlEncodedContent(values);
        Client.PostAsync("/Littlefield/CheckAccess", content).Wait();
    }

    // TODO: Make page loads async
    public static Dictionary<Stations, Dictionary<int, double>> GetQueues()
    {
        var plots = new List<Stations> { Stations.JOBIN, Stations.JOBQ, Stations.INV, Stations.S1Q, Stations.S1UTIL, Stations.S2Q, Stations.S2UTIL, Stations.S3Q, Stations.S3UTIL };

        Dictionary<Stations, Dictionary<int, double>> ans = plots.ToDictionary(s => s, s => GetPlot(s));

        var resPlots = new List<Stations> { Stations.JOBOUT, Stations.JOBT, Stations.JOBREV };
        var resAns = resPlots.ToDictionary(
                n => n,
                n => GetResults(n)
                    .GroupBy(d => d.Key)
                    .ToDictionary(d => d.Key, d => d.Sum(v => v.Value)));

        return ans.Concat(resAns).ToDictionary(i => i.Key, i => i.Value);
    }

    private static Dictionary<int, double> GetResults(Stations key)
    {
        var data = RequestData($"/Littlefield/Plot?data={Enum.GetName(typeof(Stations), key)}&x=all");

        var ans = new Dictionary<string, Dictionary<int, double>>();
        var pattern = new Regex("{label: '.*?},", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        var matches = pattern.Matches(data).ToList();

        var groups = matches
            .SelectMany(m =>
                Extract(m.Value, ", points: '", "'},")
                .Split(" ")
                .Select((v, i) => new
                {
                    Value = double.Parse(v),
                    Index = int.Parse(i.ToString())
                })
                .Where(i => i.Index % 2 == 1));

        return groups.GroupBy(a => a.Index).ToDictionary(g => g.Key / 2 + 1, i => i.Sum(a => a.Value));
    }

    public static Dictionary<string, string> GetBase()
    {
        var ans = new Dictionary<string, string>();

        foreach (var source in InformationSources.DynamicDataSources)
        {
            string responseFromServer = RequestData(source.Path);

            foreach (Match match in source.Regex.Matches(responseFromServer))
            {
                ans.Add($"[{source.Name}] {match.Groups["name"].Value}", match.Groups["value"].Value.Trim());
            }
        }

        foreach (var source in InformationSources.ListedDataSources)
        {
            string responseFromServer = RequestData(source.Key);
            ans = ans.Union(source.Value(responseFromServer)).ToDictionary(d => d.Key, d => d.Value);
        }
        return ans;
    }
}