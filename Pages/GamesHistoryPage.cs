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
            else if (Game.Equals("current"))
            {
                SteamGameID = 0;
                title = "";
            }
            else
            {
                BaseFuncs.Show404(client);
                return false;
            }

            bool MyLots = false;

            XLottery[] Lots = new XLottery[0];
            List<XLottery> LotteryList = new List<XLottery>();

            int ShowNum = 10;
            int from = 0;

            if(client.GetParam("from") != null)
            {
                int.TryParse(client.GetParam("from"), out from);

                if(from < 0)
                {
                    from = 0;
                }
            }
            int CurrentTime = Helper.GetCurrentTime();

            XUser user;
            if (urls.Length > 1 && urls[1].Equals("my") && Helper.UserHelper.GetCurrentUser(client, out user))
            {
                Helper.LotteryHelper.Table.SelectArrFromEnd(data =>
                {
                    if (((SteamGameID == 0) ? true : data.SteamGameID == SteamGameID) && data.WinnersToken > 0 && (data.EndTime + 30) < CurrentTime)
                    {
                        XLotteryBet b;
                        if (Helper.LotteryHelper.TableBet.SelectOne(bt => bt.LotteryID == data.ID && bt.UserID == user.ID, out b))
                        {
                            return true;
                        }
                    }
                    return false;
                }, out Lots);

                MyLots = true;
            }
            else
            {
                Helper.LotteryHelper.Table.SelectArrFromEnd(data => ((SteamGameID == 0) ? true : data.SteamGameID == SteamGameID) && data.WinnersToken > 0 && (data.EndTime + 30) < CurrentTime, out Lots);
            }

            if(from >= Lots.Length)
            {
                from = (Lots.Length / ShowNum) * ShowNum;
            }

            ushort currency = Helper.UserHelper.GetCurrency(client);
            for (int i = from; i < Math.Min(from + ShowNum, Lots.Length); i++)
            {
                if(currency == 1)
                {
                    Lots[i].JackpotPrice *= Lots[i].RubCurrency;
                }              

                LotteryList.Add(Lots[i]);
            }

            Hashtable page_data = new Hashtable();

            page_data.Add("Title", "История лотерей " + title);
            page_data.Add("MyLots", MyLots);
            page_data.Add("Lotteries", LotteryList);
            page_data.Add("SteamGameID", SteamGameID);
            page_data.Add("GameURL", Game);
            page_data.Add("From", from);
            page_data.Add("ShowNum", ShowNum);
            page_data.Add("Currency", currency);

            page_data.Add("GamesNum", Lots.Length);
            client.HttpSend(TemplateActivator.Activate(this, client, page_data));
            return true;
        }
    }
}
