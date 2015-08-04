using GameSlot.Database;
using GameSlot.Pages;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Helpers
{
    public class GroupHelper
    {
        public GroupHelper()
        { 
        }

        public bool SelectByID(uint id, out UGroup group)
        {
            XUser user;
            if(Helper.UserHelper.Table.SelectByID(id, out user))
            {
                group = new UGroup();
                group.ID = user.ID;
                group.Name = user.GroupName != null && user.GroupName != "" ? user.GroupName : user.Name;
                group.Users = this.GetUsers(user.ID);
                group.UserCount = group.Users.Count();

                return true;
            }

            group = null;
            return false;
        }

        public bool GroupExist(uint id)
        {
            UGroup group;
            return this.SelectByID(id, out group);
        }

        public List<XUser> GetUsers(uint GroupID)
        {
            List<XUser> Users;
            Helper.UserHelper.Table.Select(data => data.GroupOwnerID == GroupID, out Users);
            return Users;
        }

        public bool EnterToGroup(uint GroupID, Client client)
        {
            XUser user;
            if(this.GroupExist(GroupID) && Helper.UserHelper.GetCurrentUser(client, out user) && user.GroupOwnerID != GroupID)
            {
                user.GroupOwnerID = Convert.ToInt32(GroupID);
                Helper.UserHelper.Table.UpdateByID(user, user.ID);

                client.Session["GroupOwnerID"] = GroupID;
                return true;
            }

            return false;
        }

        public bool UpdateNameByID(uint id, string GroupName)
        {
            XUser user_GroupID;
            if (Helper.UserHelper.Table.SelectByID(id, out user_GroupID))
            {
                if (GroupName != user_GroupID.GroupName && GroupName.Length > 2)
                {
                    user_GroupID.GroupName = BaseFuncs.XSSReplacer(GroupName);
                    Helper.UserHelper.Table.UpdateByID(user_GroupID, user_GroupID.ID);
                    return true;
                }
            }
            return false;
        }

        public void WS_UpdateGroupData(uint GroupID)
        {
            UGroup group;
            if (this.SelectByID(GroupID, out group))
            {
                foreach (Client client in BaseFuncs.GetWebsocketClients<SiteGameSlot>("/group/" + GroupID))
                {
                    XUser user;
                    int UsersGroupID = -1;
                    if(Helper.UserHelper.GetCurrentUser(client, out user))
                    {
                        UsersGroupID = user.GroupOwnerID;
                    }

                    // 0: action, 1:name, 2: usercouent, 3: group_id, 4: my_group_id
                    client.SendWebsocket("UpdateGroupData" + BaseFuncs.WSplit + group.Name + BaseFuncs.WSplit + group.UserCount + BaseFuncs.WSplit + group.ID + BaseFuncs.WSplit + UsersGroupID);
                }
            }
        }
    }
}
