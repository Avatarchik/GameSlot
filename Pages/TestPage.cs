using GameSlot.Database;
using GameSlot.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class TestPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/test"; }
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
            XSteamItem XSteamItemDOTA;
            string Test = "";
            for (int i = 0; i < 1; i++)
            {
                foreach (XSteamItem Item in Helper.SteamItemsHelper.Table.SelectAll())
                {
                    Test += Item.Name + " [" + Item.Price + "]<br />";
                }
            }

            client.HttpSend(Helper.SteamItemsHelper.Table.SelectAll().Count * 2 + "<hr />" + Test);
            return true;
        }
    }
}
