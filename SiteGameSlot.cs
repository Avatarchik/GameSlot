using GameSlot.Helpers;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;
using GameSlot.Database;

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
            get { return "uptrade.local;"; }
        }
        public override uint CacheTime
        {
            get { return 0; }
        }
        public override ushort AccessLevel
        {
            get { return 0; }
        }
        public static void OnLoad()
        {
            Helper.LotteryHelper = new LotteryHelper();
            Helper.ItemsSchemaHelper = new SteamItemsHelper();
            Helper.UserHelper = new UserHelper();
            Helper.GroupHelper = new GroupHelper();

            Helper.LotteryHelper.CreateNew(Configs.DOTA2_STEAM_GAME_ID);
            Helper.LotteryHelper.CreateNew(Configs.CSGO_STEAM_GAME_ID);

            Helper.ItemsSchemaHelper.InsertItemsDOTA();

            Logger.ConsoleLog("GAMESLOT LOTTERY site loaded!", ConsoleColor.Yellow);
        }
        public override bool PreInit(Client client)
        {
            //List<SteamItem> Items;
            //Helper.UserHelper.GetSteamInventory_Dota2(76561198134764617, out Items);
            //XItemsShemaDOTA ii;
           // Logger.ConsoleLog(Helper.ItemsSchemaHelper.TableShemaDOTA.SelectOne(data => data.DefIndex == 6299, out ii));
            //Logger.ConsoleLog(ii.Name);
            return true;
        }

        public override bool AfterInit(Client client)
        {
            client.Session["Referer"] = client.URL;
            return true;
        }
    }
}
