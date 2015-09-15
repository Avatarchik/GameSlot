using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class TopPlayersPage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/top-players/"; }
        }
        public override string TemplateAddr
        {
            get { return "TopPlayers.html"; }
        }
        public override bool Init(Client client)
        {
            ushort currency = Helper.UserHelper.GetCurrency(client);

            uint SteamGameID = 0;
            string[] urls = BaseFuncs.GetAdditionalURLArray(client.URL, this.URL);
            string Game = urls[0];

            List<XUser> Users = Helper.UserHelper.Table.SelectAll();
            string title = "";
            if (Game.Equals("dota2"))
            {
                SteamGameID = Configs.DOTA2_STEAM_GAME_ID;
                Users = (from it in Users orderby it.DOTA_RUB_WonTotalPrice descending select it).ToList();
                title = "Топ победителей DOTA2";
            }
            else if (Game.Equals("csgo"))
            {
                SteamGameID = Configs.CSGO_STEAM_GAME_ID;
                Users = (from it in Users orderby it.CSGO_RUB_WonTotalPrice descending select it).ToList();
                title = "Топ победителей CSGO";
            }
            else if (Game.Equals("current"))
            {
                Users = (from it in Users orderby (it.CSGO_RUB_WonTotalPrice + it.DOTA_RUB_WonTotalPrice) descending select it).ToList();
                title = "Топ победителей";
            }
            else
            {
                BaseFuncs.Show404(client);
                return false;
            }

            int ShowNum = 10;
            int from = 0;

            if(client.GetParam("from") != null)
            {
                int.TryParse(client.GetParam("from"), out from);
            }

            if (from >= Users.Count)
            {
                from = (Users.Count / ShowNum) * ShowNum;
            }


            List<TopPlayer> TopPlayers = new List<TopPlayer>();
            for (int i = from; i < Math.Min(from + ShowNum, Users.Count); i++)
            {
                if(SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                {
                    if(Users[i].CSGO_WonTotalPrice <= 0)
                    {
                        continue;
                    }
                }
                else if (SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                {
                    if (Users[i].DOTA_WonTotalPrice <= 0)
                    {
                        continue;
                    }
                }
                else if (SteamGameID == 0)
                {
                    if ((Users[i].CSGO_WonTotalPrice + Users[i].DOTA_WonTotalPrice) <= 0)
                    {
                        continue;
                    }
                }
                TopPlayer TopPlayer = new TopPlayer();
                TopPlayer.ID = Users[i].ID;
                TopPlayer.Name = Users[i].Name;
                TopPlayer.Avatar = Users[i].Avatar;

                if (SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                {
                    TopPlayer.GamesCount = Users[i].DOTA_GamesCount;
                    TopPlayer.WonCount = Users[i].DOTA_WonCount;

                    TopPlayer.BetItemsCount = Users[i].DOTA_TotalBetItemsCount;
                    TopPlayer.WonItemsCount = Users[i].DOTA_WonItemsCount;

                    TopPlayer.BetPrice = Users[i].DOTA_RUB_TotalBetPrice;
                    TopPlayer.WonPrice = Users[i].DOTA_RUB_WonTotalPrice;
                }

                else if (SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                {
                    TopPlayer.GamesCount = Users[i].CSGO_GamesCount;
                    TopPlayer.WonCount = Users[i].CSGO_WonCount;

                    TopPlayer.BetItemsCount = Users[i].CSGO_TotalBetItemsCount;
                    TopPlayer.WonItemsCount = Users[i].CSGO_WonItemsCount;

                    TopPlayer.BetPrice = Users[i].CSGO_RUB_TotalBetPrice;
                    TopPlayer.WonPrice = Users[i].CSGO_RUB_WonTotalPrice;
                }

                else if (SteamGameID == 0)
                {
                    TopPlayer.GamesCount = (Users[i].CSGO_GamesCount + Users[i].DOTA_GamesCount);
                    TopPlayer.WonCount = (Users[i].CSGO_WonCount + Users[i].DOTA_WonCount);

                    TopPlayer.BetItemsCount = (Users[i].CSGO_TotalBetItemsCount + Users[i].DOTA_TotalBetItemsCount);
                    TopPlayer.WonItemsCount = (Users[i].CSGO_WonItemsCount + Users[i].DOTA_WonItemsCount);

                    TopPlayer.BetPrice = (Users[i].CSGO_RUB_TotalBetPrice + Users[i].DOTA_RUB_TotalBetPrice);
                    TopPlayer.WonPrice = (Users[i].CSGO_RUB_WonTotalPrice + Users[i].DOTA_RUB_WonTotalPrice);
                }

                if (TopPlayer.WonCount > 0)
                {
                    TopPlayer.Winrate = (int)(TopPlayer.WonCount / ((double)TopPlayer.GamesCount / 100d));
                }
                else
                {
                    TopPlayer.Winrate = 0;
                }

                TopPlayer.BetPrice_Str = TopPlayer.BetPrice.ToString("###,###,##0");
                TopPlayer.WonPrice_Str = TopPlayer.WonPrice.ToString("###,###,##0");

                TopPlayers.Add(TopPlayer);
            }

            Hashtable data = new Hashtable();

            data.Add("SteamGameID", SteamGameID);
            data.Add("Users", TopPlayers);
            data.Add("GameURL", Game);
            data.Add("From", from);
            data.Add("ShowNum", ShowNum);
            data.Add("UsersNum", TopPlayers.Count);
            data.Add("Title", title);
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
