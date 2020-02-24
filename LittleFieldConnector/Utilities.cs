using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class Utilities
{
    static Utilities()
    {
        Authenticate();
    }

    public static string Extract(string contents, string before, string after = null)
    {
        after = after ?? "\n";

        var upToStart = contents.Substring(contents.IndexOf(before) + before.Length);

        return upToStart.Substring(0, upToStart.IndexOf(after));
    }

    public static Dictionary<string, dynamic> Items(string contents, string itemPatern, string itemLabel, Func<string, string> getId, Dictionary<string, Func<string, dynamic>> actions)
    {
        var ans = new Dictionary<string, dynamic>();
        var pattern = new Regex(itemPatern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        var matches = pattern.Matches(contents).ToList();

        if (matches.Count == 0)
        {
            throw new Exception("No matches found");
        }

        foreach (Match matchContents in matches)
        {
            var id = getId(matchContents.Value);
            foreach (var action in actions)
            {
                ans.Add($"{itemLabel} {id}: {action.Key}", action.Value(matchContents.Value));
            }
        }
        return ans;
    }

    public static int AsInt(string value)
    {
        return int.Parse(value.Replace(",", ""));
    }

    public static double AsDouble(string value)
    {
        return double.Parse(value);
    }

    public static string RequestData(string path)
    {
        var request = Client.GetAsync(path).Result;

        return request.Content.ReadAsStringAsync().Result;
    }

    private static CookieContainer Cookies = new CookieContainer();
    private static HttpClientHandler Handler = new HttpClientHandler() { CookieContainer = Cookies, AllowAutoRedirect = true, UseCookies = true };

    private static HttpClient Client = new HttpClient(Handler)
    {
        BaseAddress = new Uri("http://op.responsive.net/")
    };

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
}