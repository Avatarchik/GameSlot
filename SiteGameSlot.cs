using GameSlot.Helpers;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;
using GameSlot.Database;
using System.IO;
using System.Threading;

namespace GameSlot
{
    public class SiteGameSlot : Page
    {
        public override CacheLevel CacheLevel
        {
            get { return CacheLevel.NoCache; }
        }
        public override PageType PageType
        {
            get { return PageType.Once; }
        }
        public override bool CanAccessDirectly
        {
            get { return true; }
        }
        public override bool FilterBefore
        {
            get { return true; }
        }
        public override bool FilterAfter
        {
            get { return true; }
        }
        public override bool EnableHooking
        {
            get { return false; }
        }
        public override string URL
        {
            get { return null; }
        }
        public override string TemplateAddr
        {
            get { return null; }
        }
        public override string Host
        {
            //get { return "localhost;uptrade.local;gameslot.uptrade.pro;thegameslot.ru;www.thegameslot.ru;thegameslot.com;www.thegameslot.com"; }
           // get { return "localhost;uptrade.local;gameslot.uptrade.pro;thegameslot.com;www.thegameslot.com"; }
            get { return "localhost;uptrade.local;gameslot.uptrade.pro;thegameslot.com;www.thegameslot.com"; }
        }
        public override uint CacheTime
        {
            get { return 0; }
        }
        public override ushort AccessLevel
        {
            get { return 0; }
        }
        public override bool MaintenanceAffect
        {
            get { return true; }
        }
        public static void OnLoad()
        {
            ServerData.LogLevel = LogLevel.Debug;

            Logger.ConsoleLog("Getting rub currency...");
            Helper.Rub_Rate();
            while(Helper.Rub_ExchangeRate <= 0)
            {
                Thread.Sleep(100);
            }

            Helper.LotteryHelper = new LotteryHelper();
            Helper.SteamItemsHelper = new SteamItemsHelper();
            Helper.UserHelper = new UserHelper();
            Helper.GroupHelper = new GroupHelper();
            Helper.ChipHelper = new ChipHelper();
            Helper.SteamBotHelper = new SteamBotHelper();
            Helper.OrderHelper = new OrderHelper();

            Helper.LotteryHelper.StartLottery(Configs.DOTA2_STEAM_GAME_ID);
            Thread.Sleep(100);
            Helper.LotteryHelper.StartLottery(Configs.CSGO_STEAM_GAME_ID);

            //Helper.UserHelper.UpdateOnlineUsersInventory(Configs.DOTA2_STEAM_GAME_ID);
            //Helper.UserHelper.UpdateOnlineUsersInventory(Configs.CSGO_STEAM_GAME_ID);

            Helper.SteamItemsHelper.UpdatePrices(Configs.DOTA2_STEAM_GAME_ID);
            Helper.SteamItemsHelper.UpdatePrices(Configs.CSGO_STEAM_GAME_ID);

            Logger.ConsoleLog("GAMESLOT LOTTERY site loaded!", ConsoleColor.Yellow);
        }
        public override bool PreInit(Client client)
        {

            if (client.Session["Currency"] == null)
            {
                client.Session["Currency"] = 1;
            }

            return true;
        }

        public override bool AfterInit(Client client)
        {
            client.Session["Referer"] = client.URL;
            //Logger.ConsoleLog(client.Session["Referer"]);
            return true;
        }
    }
}
