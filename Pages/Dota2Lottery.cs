using GameSlot.Database;
using GameSlot.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class Dota2Lottery : SiteGameSlot
    {
        public override string URL
        {
            get { return "/dota2"; }
        }
        public override string TemplateAddr
        {
            get { return "Lottery.html"; }
        }
        public override bool FilterBefore
        {
            get { return true; }
        }
        public override bool Init(Client client)
        {
            Hashtable data = new Hashtable();
            XLottery lottery = Helper.LotteryHelper.GetCurrent(Configs.DOTA2_STEAM_GAME_ID);

            data.Add("Lottery", lottery);
            data.Add("LeftTime", Helper.LotteryHelper.CalcLeftTime(lottery.ID));

            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
