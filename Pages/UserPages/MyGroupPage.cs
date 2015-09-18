using GameSlot.Database;
using GameSlot.Pages.Includes;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.UserPages
{
    public class MyGroupPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/my.hangout"; }
        }
        public override string TemplateAddr
        {
            get { return "UserPages.MyGroup.html"; }
        }
        public override bool Init(Client client)
        {
            XUser User;
            if (Helper.UserHelper.GetCurrentUser(client, out User))
            {
                Logger.ConsoleLog("DOTA: DOTA_GroupWonTotalPrice: " + User.DOTA_GroupWonTotalPrice);
                Logger.ConsoleLog("DOTA: DOTA_RUB_GroupWonTotalPrice: " + User.DOTA_RUB_GroupWonTotalPrice);
                Logger.ConsoleLog("DOTA: DOTA_GroupWonItemsCount: " + User.DOTA_GroupWonItemsCount);

                Logger.ConsoleLog("CSGO: CSGO_GroupWonTotalPrice: " + User.CSGO_GroupWonTotalPrice);
                Logger.ConsoleLog("CSGO: CSGO_RUB_GroupWonTotalPrice: " + User.CSGO_RUB_GroupWonTotalPrice);
                Logger.ConsoleLog("CSGO: CSGO_GroupWonItemsCount: " + User.CSGO_GroupWonItemsCount);

                UGroup group;
                Helper.GroupHelper.SelectByID(User.ID, out group, out User, client);

                Hashtable data = new Hashtable();
                data.Add("Group", group);
                data.Add("Title", "Моя тусовка");
                client.HttpSend(TemplateActivator.Activate(this, client, data));
                return true;
            }
            BaseFuncs.Show403(client);
            return false;
        }
    }
}
