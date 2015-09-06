using GameSlot.Database;
using GameSlot.Helpers;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Lotteries
{
    public class D2Lottery : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/game/"; }
        }
        public override string TemplateAddr
        {
            get { return "Lotteries.Lottery.html"; }
        }
        public override bool FilterBefore
        {
            get { return true; }
        }
        public override bool Init(Client client)
        {
            uint SteamGameID = 0;
            string Game = BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0];

            string title;
            if (Game.Equals("dota2"))
            {
                SteamGameID = Configs.DOTA2_STEAM_GAME_ID;
                title = "DOTA2";
            }
            else if (Game.Equals("csgo"))
            {
                SteamGameID = Configs.CSGO_STEAM_GAME_ID;
                title = "CSGO";
            }
            else
            {
                BaseFuncs.Show404(client);
                return false;
            }

            Hashtable data = new Hashtable();

            XUser user;
            if (Helper.UserHelper.GetCurrentUser(client, out user))
            {
                double ChipsTotalPrice;
                data.Add("Chips", Helper.UserHelper.GetChipInventory(user.ID, out ChipsTotalPrice));
                data.Add("ChipsTotalPrice", ChipsTotalPrice.ToString("###,##0.00"));

                data.Add("User", user);
            }

            Lottery Lottery = Helper.LotteryHelper.GetCurrent(SteamGameID, client);
            data.Add("Lottery", Lottery);

            data.Add("TopItems", Helper.LotteryHelper.GetTopItems(Lottery.ID, 7, client));
            data.Add("Bets", Helper.LotteryHelper.GetBets(Lottery.ID, client));

            int items;
            data.Add("MaxJackpot", Helper.LotteryHelper.MaxJackpot(SteamGameID, out items).ToString("###,##0.00"));
            data.Add("MaxJackpotItems", items);

            double TodaysJackpotPrice;
            int TodaysJackpotItems;
            data.Add("TodaysGames", Helper.LotteryHelper.TodaysGames(SteamGameID, out TodaysJackpotPrice, out TodaysJackpotItems).Length);
            data.Add("TodaysJackpotItems", TodaysJackpotItems);
            data.Add("TodaysJackpotPrice", TodaysJackpotPrice.ToString("###,##0.00"));

            data.Add("Title", "Лотерея " + title);
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
