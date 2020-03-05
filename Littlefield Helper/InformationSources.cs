using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Utilities;

public static class InformationSources
{
    public static List<Source> DynamicDataSources = new List<Source>
        {
            new Source {
                Name = "Orders",
                Path = "http://op.responsive.net/Littlefield/OrdersMenu",
                Regex = new Regex(@"<B> ?(?<name>[\w\d\s]+): <\/B>(?<value>[\d\w\s$,]+)")
            },
            new Source {
                Name = "Status",
                Path = "http://op.responsive.net/Littlefield/LTStatus",
                Regex = new Regex(@"<b> ?(?<name>[\w\d\s]+): <\/b>(?<value>[\d\w\s$,]+)")
            },
            new Source {
                Name = "Materials",
                Path = "http://op.responsive.net/Littlefield/MaterialMenu",
                Regex = new Regex(@"<B> ?(?<name>[\w\d\s-]+): ?<\/B>(?<value>[\d\w\s$,()]+)")
            },
            new Source {
                Name = "S1",
                Path = "http://op.responsive.net/Littlefield/StationMenu?id=1",
                Regex = new Regex(@"<B> ?(?<name>[\w\d\s]+): <\/B>(?<value>[\d\w\s$,]+)")
            },
            new Source {
                Name = "S2",
                Path = "http://op.responsive.net/Littlefield/StationMenu?id=2",
                Regex = new Regex(@"<B> ?(?<name>[\w\d\s]+): <\/B>(?<value>[\d\w\s$,]+)")
            },
            new Source {
                Name = "S3",
                Path = "http://op.responsive.net/Littlefield/StationMenu?id=3",
                Regex = new Regex(@"<B> ?(?<name>[\w\d\s]+): <\/B>(?<value>[\d\w\s$,]+)")
            }
        };


    public static Dictionary<string, Func<string, Dictionary<string, string>>> ListedDataSources =
        new Dictionary<string, Func<string, Dictionary<string, string>>>
        {
                {
                    "http://op.responsive.net/Littlefield/OrdersForm",
                    p => Items(p, @"<table>.*?</table>", "Contract",
                        c => Extract(c, "<TD><U>Contract ", "</U><BR>"),
                        new Dictionary<string, Func<string, string>>{
                            { "Quoted Lead Time", s => Extract(s, "Quoted lead time: ", " day(s)<BR>") },
                            { "Maximum Lead Time", s => Extract(s, "Maximum lead time: ", " day(s)<BR>") },
                            { "Revenue / Order", s => Extract(s, "Revenue per order: ", " dollars") },
                        })
                }
        };
}

public class Source
{
    public string Path { get; set; }
    public string Name { get; set; }
    public Regex Regex { get; set; }
}
