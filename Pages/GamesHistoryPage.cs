using GameSlot.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class GamesHistoryPage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/games-history/"; }
        }
        public override string TemplateAddr
        {
            get { return "GamesHistory.html"; }
        }
        public override bool Init(Client client)
        {
            uint SteamGameID = 0;
            string[] urls = BaseFuncs.GetAdditionalURLArray(client.URL, this.URL);
            string Game = urls[0];

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

            XLottery[] Lotteries;
            XUser User;
            if (urls.Length > 1 && urls[1].Equals("my") && Helper.UserHelper.GetCurrentUser(client, out User))
            {
                int count = 0;
                Helper.LotteryHelper.Table.SelectArrFromEnd(data => 
                {
                    if (count < 10)
                    {
                        if (data.SteamGameID == SteamGameID && data.WinnersToken > 0)
                        {
                            XLotteryBet bet;
                            if (Helper.LotteryHelper.TableBet.SelectOne(b => b.LotteryID == data.ID && b.UserID == User.ID, out bet))
                            {
                                count++;
                                return true;
                            }
                        }
                    }
                    return false;
                }, out Lotteries);
            }
            else
            {
                Helper.LotteryHelper.Table.SelectArrFromEnd(data => data.SteamGameID == SteamGameID && data.WinnersToken > 0, out Lotteries, 0, 11);
            }

            Hashtable page_data = new Hashtable();
            page_data.Add("Title", "История лотерей " + title);
            page_data.Add("Lotteries", Lotteries);
            page_data.Add("SteamGameID", SteamGameID);
            client.HttpSend(TemplateActivator.Activate(this, client, page_data));
            return true;
        }
    }
}
