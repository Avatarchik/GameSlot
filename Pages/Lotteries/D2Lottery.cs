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
            else if(Game.Equals("teamfortess"))
            {
                BaseFuncs.ShowPage(new CommingSoonPage(), client);
                return true;
            }
            else
            {
                BaseFuncs.Show404(client);
                return false;
            }

            Hashtable data = new Hashtable();
            ushort currency = Helper.UserHelper.GetCurrency(client);

            Lottery Lottery = Helper.LotteryHelper.GetCurrent(SteamGameID, client);

            XUser user;
            bool auth = Helper.UserHelper.GetCurrentUser(client, out user);
            if (auth)
            {
                double ChipsTotalPrice;
                data.Add("Chips", Helper.UserHelper.GetChipInventory(user.ID, out ChipsTotalPrice));

                if (user.Currency == 1)
                {
                    data.Add("ChipsTotalPrice", ChipsTotalPrice.ToString("###,###,##0"));
                }
                else
                {
                    data.Add("ChipsTotalPrice", ChipsTotalPrice.ToString("###,##0.00"));
                }

                data.Add("User", user);
            }

            if (auth && Lottery.LeftTime == 0 && user.ID == Lottery.Winner.ID)
            {
                data.Add("ItsWinner", true);
            }
            else
            {
                data.Add("ItsWinner", false);
            }

            data.Add("Lottery", Lottery);

            data.Add("TopItems", Helper.LotteryHelper.GetTopItems(Lottery.ID, 7, client));
            data.Add("Bets", Helper.LotteryHelper.GetBets(Lottery.ID, client));

            double TodaysJackpotPrice;
            int TodaysJackpotItems;
            data.Add("TodaysGames", Helper.LotteryHelper.TodaysGames(SteamGameID, out TodaysJackpotPrice, out TodaysJackpotItems, currency).Length);

            int items;
            if (currency == 1)
            {
                data.Add("MaxJackpot", Helper.LotteryHelper.MaxJackpot(SteamGameID, out items, 1).ToString("###,###,##0"));
                data.Add("TodaysJackpotPrice", TodaysJackpotPrice.ToString("###,###,##0"));
            }
            else
            {
                data.Add("MaxJackpot", Helper.LotteryHelper.MaxJackpot(SteamGameID, out items, 0).ToString("###,##0.00"));
                data.Add("TodaysJackpotPrice", TodaysJackpotPrice.ToString("###,##0.00"));
            }

            data.Add("MaxJackpotItems", items);
            data.Add("TodaysJackpotItems", TodaysJackpotItems);
            
            data.Add("Title", "Лотерея " + title);
            data.Add("Game", Game);
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
