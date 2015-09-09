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
            XUser User;
            if (uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0], out id) && Helper.GroupHelper.SelectByID(id, out Group, out User, client))
            {
                Hashtable data = new Hashtable();
                data.Add("Owner", User);

                bool InGroup = Helper.UserHelper.GetCurrentUser(client, out User) && User.GroupOwnerID == Group.ID ? true : false;
                bool InOtherGroup = !InGroup && User.GroupOwnerID >= 0 ? true : false;

                data.Add("Group", Group);
                data.Add("InGroup", InGroup);
                data.Add("InOtherGroup", InOtherGroup);

                client.HttpSend(TemplateActivator.Activate(this, client, data));
                return true;
            }

            BaseFuncs.Show404(client);
            return false;
        }

        public int GetGroupWinrate(uint GroupID)
        {
            int winrate = 0;
            int count = 0;
            //uint totalprice
            XLottery[] Lotteries;
            

            return winrate;
        }
    }
}
