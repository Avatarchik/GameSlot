using GameSlot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot
{
    public class Helper
    {
        public static LotteryHelper LotteryHelper;
        public static SteamItemsHelper ItemsSchemaHelper;
        public static UserHelper UserHelper;
        public static GroupHelper GroupHelper; 

        public static int GetCurrentTime()
        {
            return (int)(DateTime.Now - new DateTime(2015, 7, 18)).TotalSeconds;
        }

        public static string GetReferer(Client client)
        {
            return client.Session["Referer"] != null ? (string)client.Session["Referer"] : "/";
        }
    }
}
