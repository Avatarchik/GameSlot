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

        public static double Rub_ExchangeRate = 0;
        public static int OnlineUsers = 0;

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
                        Rub_ExchangeRate = Math.Round(double.Parse(Regex.Split(wc.DownloadString("http://www.cbr.ru/scripts/XML_daily.asp"), "R01235")[1].Split('>')[10].Split('<')[0].Replace(',', '.')));
                    }
                    Thread.Sleep(TimeSpan.FromHours(12));
                }
                catch { }
            }).Start();
        }

        public static string MakeDichFromCuteText(string cute_text)
        {
            StringBuilder dich = new StringBuilder();
            byte[] Data = Encoding.Unicode.GetBytes(cute_text);
            for (int i = 0; i < Data.Length; i++)
            {
                string val1 = Data[i++].ToString("X2");
                string val2 = Data[i].ToString("X2");

                dich.Append(@"\u" + val1 + val2);
            }
            return dich.ToString();
        }
    }
}
