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
    public class MainPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/"; }
        }
        public override string TemplateAddr
        {
            get { return "Main.html"; }
        }
        public override bool Init(Client client)
        {

            Hashtable data = new Hashtable();
            data.Add("Lottery_CSGO", Helper.LotteryHelper.GetCurrent(Configs.CSGO_STEAM_GAME_ID, client));
            data.Add("Lottery_DOTA", Helper.LotteryHelper.GetCurrent(Configs.DOTA2_STEAM_GAME_ID, client));

            int today_i_d2, today_i_cs;
            double today_p_d2, today_p_cs;
            data.Add("GameToday_CSGO", Helper.LotteryHelper.TodaysGames(Configs.CSGO_STEAM_GAME_ID, out today_p_cs, out today_i_cs).Length);
            data.Add("GameToday_DOTA", Helper.LotteryHelper.TodaysGames(Configs.DOTA2_STEAM_GAME_ID, out today_p_d2, out today_i_d2).Length);

            int i_d2, i_cs;
            data.Add("MaxJackpot_CSGO", Helper.LotteryHelper.MaxJackpot(Configs.CSGO_STEAM_GAME_ID, out i_cs).ToString("###,##0.00"));
            data.Add("MaxJackpot_DOTA", Helper.LotteryHelper.MaxJackpot(Configs.DOTA2_STEAM_GAME_ID, out i_d2).ToString("###,##0.00"));

            data.Add("Gamers_CSGO", "");
            data.Add("Gamers_DOTA", "");

            data.Add("Title", "GAMESLOT - поднимайся на ставках!");

            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
