using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Utilities;

namespace LittleFieldConnector
{
    public static class InformationSources
    {
        public static Dictionary<string, Dictionary<string, Func<string, dynamic>>> DataSources = new Dictionary<string, Dictionary<string, Func<string, dynamic>>>
        {
            {
                "http://op.responsive.net/Littlefield/OrdersMenu",
                new Dictionary<string, Func<string, dynamic>>
                {
                    { "WIP Limit (Jobs)*", c => AsInt(Extract(c, "<b>Maximum WIP Limit: </b>", " jobs<BR>")) },
                    { "Kits / Job", c => AsInt(Extract(c, "<B>Number of kits in 1 job: </B>", "<BR>")) },
                    { "Lot Size (Kits)*", c => AsInt(Extract(c, "<B>Lot size: </B>", " kits,")) },
                    { "Current Contract*", c => AsInt(Extract(c, "<B>Current contract: </B>","<BR>")) },
                    { "Current Contract: Quoted Lead Time", c => AsDouble(Extract(c, "<DD>Quoted lead time: "," day(s)<BR>")) },
                    { "Current Contract: Maximum Lead Time", c => AsDouble(Extract(c, "<DD>Maximum lead time: "," day(s)<BR>")) },
                    { "Current Contract: Revenue / Order", c => AsDouble(Extract(c, "<DD>Revenue per order: "," dollars<BR><HR>")) },
                }
            },
            {
                "http://op.responsive.net/Littlefield/LTStatus",
                new Dictionary<string, Func<string, dynamic>>
                {
                    { "Day", c => AsInt(Extract(c, "<b>Day: </b> ")) },
                    { "Cash Balance", c => AsInt(Extract(c, "Cash Balance: </b> ")) },
                }
            },
            {
                "http://op.responsive.net/Littlefield/MaterialMenu",
                new Dictionary<string, Func<string, dynamic>>
                {
                    { "Materials: Unit Cost", c => AsDouble(Extract(c, "<BR><B>Unit Cost: </B> $ ")) },
                    { "Materials: Order Cost", c => AsDouble(Extract(c, "<BR><B>Order Cost: </B> $ ")) },
                    { "Materials: Lead Time", c => AsInt(Extract(c, "<BR><B>Lead Time:</B> ", " day(s)")) },
                    { "Materials: Reorder Point (kits)*", c => AsInt(Extract(c, "<BR><B>Reorder Point:</B> ", " kits")) },
                    { "Materials: Order Quantity (kits)*", c => AsInt(Extract(c, "<BR><B>Order Quantity:</B>\n", " kits")) }
                }
            },
            {
                "http://op.responsive.net/Littlefield/StationMenu?id=1",
                new Dictionary<string, Func<string, dynamic>>
                {
                    { "S1: Machines*", c => AsInt(Extract(c, "<P><B> Number of Machines: </B>", "<BR>")) },
                    { "S1: Scheduling", c => Extract(c, "<B>Scheduling Policy: </B>", "<BR>") },
                    { "S1: Purchase Price", c => AsInt(Extract(c, "<B>Purchase Price: </B>$ ", "<BR>")) },
                    { "S1: Retirement Price", c => AsInt(Extract(c, "<B>Retirement Price: </B>$ ", "<BR>")) },
                }
            },
            {
                "http://op.responsive.net/Littlefield/StationMenu?id=2",
                new Dictionary<string, Func<string, dynamic>>
                {
                    { "S2/4: Machines*", c => AsInt(Extract(c, "<P><B> Number of Machines: </B>", "<BR>")) },
                    { "S2/4: Scheduling*", c => Extract(c, "<B>Scheduling Policy: </B>", "<BR>") },
                    { "S2/4: Purchase Price", c => AsInt(Extract(c, "<B>Purchase Price: </B>$ ", "<BR>")) },
                    { "S2/4: Retirement Price", c => AsInt(Extract(c, "<B>Retirement Price: </B>$ ", "<BR>")) },
                }
            },
            {
                "http://op.responsive.net/Littlefield/StationMenu?id=3",
                new Dictionary<string, Func<string, dynamic>>
                {
                    { "S3: Machines*", c => AsInt(Extract(c, "<P><B> Number of Machines: </B>", "<BR>")) },
                    { "S3: Scheduling", c => Extract(c, "<B>Scheduling Policy: </B>", "<BR>") },
                    { "S3: Purchase Price", c => AsInt(Extract(c, "<B>Purchase Price: </B>$ ", "<BR>")) },
                    { "S3: Retirement Price", c => AsInt(Extract(c, "<B>Retirement Price: </B>$ ", "<BR>")) },
                }
            }
        };

        public static Dictionary<string, Func<string, Dictionary<string, dynamic>>> ListedDataSources = 
            new Dictionary<string, Func<string, Dictionary<string, dynamic>>>
            {
                {
                    "http://op.responsive.net/Littlefield/OrdersForm",
                    p => Items(p, @"<table>.*?</table>", "Contract", c => Extract(c, "<TD><U>Contract ", "</U><BR>"), 
                        new Dictionary<string, Func<string, dynamic>>{
                            { "Quoted Lead Time", s => AsDouble(Extract(s, "Quoted lead time: ", " day(s)<BR>")) },
                            { "Maximum Lead Time", s => AsDouble(Extract(s, "Maximum lead time: ", " day(s)<BR>")) },
                            { "Revenue / Order", s => AsDouble(Extract(s, "Revenue per order: ", " dollars")) },
                        })
                }
            };
    }
}
