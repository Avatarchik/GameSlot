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

        public bool SelectByID(uint id, out UGroup group, out XUser Owner, Client client)
        {
            XUser user;
            if(Helper.UserHelper.Table.SelectByID(id, out user))
            {
                group = new UGroup();
                group.ID = user.ID;
                group.Name = user.GroupName != null && user.GroupName != "" ? user.GroupName : user.Name;
                group.Users = this.GetUsers(user.ID);
                group.UserCount = group.Users.Count();

                group.Winrate = this.CalcWinrate(group.ID);
                group.BetItemsCount = user.GroupTotalBetItemsCount;

                ushort currency = Helper.UserHelper.GetCurrency(client);
                if (currency == 1)
                {
                    group.BetPrice = user.RUB_GroupTotalBetPrice;
                    group.BetItemsPrice_Str = group.BetPrice.ToString("###,###,##0");

                    group.GotPriceFromGroup = user.RUB_GotPriceFromGroup;
                    group.GotPriceFromGroup_Str = group.GotPriceFromGroup.ToString("###,###,##0");
                }
                else
                {
                    group.BetPrice = user.GroupTotalBetPrice;
                    group.BetItemsPrice_Str = group.BetPrice.ToString("###,##0.00");

                    group.GotPriceFromGroup = user.GotPriceFromGroup;
                    group.GotPriceFromGroup_Str = group.GotPriceFromGroup.ToString("###,##0.00");
                }

                group.GotItemsFromGroup = user.GotItemsFromGroup;

                Owner = user;
                return true;
            }

            group = null;
            Owner = new XUser();
            return false;
        }

        public UGroup SelectByID(uint id)
        {
            UGroup group;
            XUser Owner;
            this.SelectByID(id, out group, out Owner, null);
            return group;
        }

        public bool GroupExist(uint id)
        {
            UGroup group;
            XUser user;
            return this.SelectByID(id, out group, out user, null);
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

        public bool ExitFromGroup(uint GroupID, Client client)
        {
            XUser user;
            if (this.GroupExist(GroupID) && Helper.UserHelper.GetCurrentUser(client, out user) && user.GroupOwnerID == GroupID)
            {
                user.GroupOwnerID = -1;
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

        public int CalcWinrate(uint id)
        {
            XUser GroupOwner;
            if (Helper.UserHelper.Table.SelectByID(id, out GroupOwner) && GroupOwner.GroupWonCount > 0 && GroupOwner.GroupGamesCount > 0)
            {

                Logger.ConsoleLog(GroupOwner.GroupWonCount + " " + GroupOwner.GroupGamesCount);
                return (int)(100 * GroupOwner.GroupWonCount / GroupOwner.GroupGamesCount);
            }

            Logger.ConsoleLog(GroupOwner.GroupWonCount + " " + GroupOwner.GroupGamesCount);
            return 0;
        }
    }
}
