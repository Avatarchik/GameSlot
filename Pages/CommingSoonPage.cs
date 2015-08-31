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
    public class CommingSoonPage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/comming-soon"; }
        }
        public override string TemplateAddr
        {
            get { return "CommingSoon.html"; }
        }
        public override bool Init(Client client)
        {
            Hashtable data = new Hashtable();
            data.Add("Title", "Comming soon... :)");
            client.HttpSend(TemplateActivator.Activate(this, client, data));
            return true;
        }
    }
}
