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
            XLottery xlot = Helper.LotteryHelper.GetCurrent(Configs.DOTA2_STEAM_GAME_ID);

            Lottery Lottery;
            Helper.LotteryHelper.GetLottery(xlot.ID, out Lottery);

            data.Add("Lottery", Lottery);
            XUser User;
            if (Helper.UserHelper.GetCurrentUser(client, out User))
            {
                data.Add("Chips", Helper.UserHelper.GetChipInventory(User.ID));
            }
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
