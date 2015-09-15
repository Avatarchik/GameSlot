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

namespace GameSlot.Pages
{
    public class GroupPage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/hangout/"; }
        }
        public override string TemplateAddr
        {
            get { return "Group.html"; }
        }
        public override bool Init(Client client)
        {
            uint id;
            UGroup Group;
            XUser Owner;
            if (uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0], out id) && Helper.GroupHelper.SelectByID(id, out Group, out Owner, client))
            {
                Hashtable data = new Hashtable();
                data.Add("Owner", Owner);

                XUser CurrentUser;
                bool InGroup = Helper.UserHelper.GetCurrentUser(client, out CurrentUser) && CurrentUser.GroupOwnerID == Group.ID ? true : false;
                bool InOtherGroup = !InGroup && CurrentUser.GroupOwnerID >= 0 ? true : false;

                data.Add("Group", Group);
                data.Add("InGroup", InGroup);
                data.Add("InOtherGroup", InOtherGroup);

                data.Add("Title", "Тусовка пользователя " + Owner.Name);
                client.HttpSend(TemplateActivator.Activate(this, client, data));
                return true;
            }

            BaseFuncs.Show404(client);
            return false;
        }
    }
}
