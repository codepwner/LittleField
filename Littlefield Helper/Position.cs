using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

public class Position
{
    public int Place { get; set; }
    public int Money { get; set; }
    public string Name { get; set; }

    public static IEnumerable<Position> Current
    {
        get
        {
            string responseFromServer = Fetch.RequestData("http://op.responsive.net/Littlefield/Standing");
            var matcher = new Regex(@"<TD .*><font .*>(?<place>.*)<\/font>\s*<TD .*><font .*>(?<name>.*)<\/font>\s*<TD .*><font .*>(?<money>.*)<\/font>");

            return matcher.Matches(responseFromServer).Select(v => new Position
            {
                Place = int.Parse(v.Groups["place"].Value),
                Name = v.Groups["name"].Value,
                Money = int.Parse(v.Groups["money"].Value.Replace(",", "").Trim())
            });
        }
    }
}