using GameSlot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot
{
    public class Helper
    {
        public static LotteryHelper LotteryHelper;
        public static SteamItemsHelper SteamItemsHelper;
        public static UserHelper UserHelper;
        public static GroupHelper GroupHelper;
        public static ChipHelper ChipHelper;
        public static SteamBotHelper SteamBotHelper;
        public static OrderHelper OrderHelper;

        public static double Rub_ExchangeRate = 64;

        public static int GetCurrentTime()
        {
            return (int)(DateTime.Now - new DateTime(2015, 7, 18)).TotalSeconds;
        }

        public static string GetReferer(Client client)
        {
            return client.Session["Referer"] != null ? (string)client.Session["Referer"] : "/";
        }

        public static void Rub_Rate()
        {
            new Thread(delegate()
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        Rub_ExchangeRate = double.Parse(Regex.Split(wc.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp"), "R01235")[1].Split('>')[10].Split('<')[0].Replace(',', '.'));
                    }
                    Thread.Sleep(TimeSpan.FromHours(12));
                }
                catch { }
            }).Start();
        }
    }
}
