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
            foreach (XSteamItem Item in Helper.SteamItemsHelper.Table.SelectAll())
            {
                Logger.ConsoleLog("------------------------");
                Logger.ConsoleLog("[" + Item.Name + "]");
                Logger.ConsoleLog("\n");
                Thread.Sleep(1000);
            }
            client.HttpSend("OK");
            return true;
        }
    }
}
