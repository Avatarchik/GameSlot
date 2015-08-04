using GameSlot.Database;
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
            get { return "/group/"; }
        }
        public override string TemplateAddr
        {
            get { return "Group.html"; }
        }
        public override bool Init(Client client)
        {
            uint id;
            UGroup Group;
            if (uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0], out id) && Helper.GroupHelper.SelectByID(id, out Group))
            {
                if (client.ConnType == ConnectionType.WebSocket && client.WSData != null)
                {
                    string[] wsdata = Regex.Split(client.WSData, BaseFuncs.WSplit);

                    if (wsdata[0].Equals("entry"))
                    {
                        XUser user;
                        if (Helper.UserHelper.GetCurrentUser(client, out user))
                        {
                            int OldGroup = user.GroupOwnerID;

                            if (Helper.GroupHelper.EnterToGroup(Group.ID, client))
                            {
                                Helper.GroupHelper.WS_UpdateGroupData(Group.ID);
                                Helper.GroupHelper.WS_UpdateGroupData(Convert.ToUInt32(OldGroup));
                            }
                        }
                    }
                    return false;
                }
                else
                {
                    XUser User;
                    bool InGroup = Helper.UserHelper.GetCurrentUser(client, out User) && User.GroupOwnerID == Group.ID ? true : false;
                    bool InOtherGroup = !InGroup && User.GroupOwnerID >= 0 ? true : false;

                    Hashtable data = new Hashtable();
                    data.Add("Group", Group);
                    data.Add("InGroup", InGroup);
                    data.Add("InOtherGroup", InOtherGroup);

                    client.HttpSend(TemplateActivator.Activate(this, client, data));
                    return true;
                }
            }

            BaseFuncs.Show404(client);
            return false;
        }
    }
}
