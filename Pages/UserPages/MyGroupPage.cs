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
            get { return "/mygroup"; }
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
                UGroup group;
                Helper.GroupHelper.SelectByID(User.ID, out group);

                Hashtable data = new Hashtable();
                data.Add("Group", group);

                client.HttpSend(TemplateActivator.Activate(this, client, data));
                return true;
            }
            BaseFuncs.Show403(client);
            return false;
        }
    }
}
