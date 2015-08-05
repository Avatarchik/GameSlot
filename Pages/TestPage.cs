using GameSlot.Database;
using GameSlot.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            //Image ing = Image.FromStream(new MemoryStream());

            List<XSteamItem> Items = Helper.SteamItemsHelper.Table.SelectAll();
            for (int i = 0; i < Items.Count; i++)
            {
                Helper.SteamItemsHelper.DownloadItemsImage(Items[i].ID, Items[i].SteamGameID);
            }

            client.HttpSend("done!!!");
            return true;
        }
    }
}
