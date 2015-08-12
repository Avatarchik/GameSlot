using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Includes
{
    public class WebSocketPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/websocket/do"; }
        }
        public override string TemplateAddr
        {
            get { return "Includes.WebSocketEvents.html"; }
        }
        public override bool FilterBefore
        {
            get { return false; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }

        private static Dictionary<uint, List<Client>> ClientsGroupPage = new Dictionary<uint, List<Client>>();

        public override bool Init(Client client)
        {
            if (client.ConnType == ConnectionType.WebSocket && client.WSData != null)
            {
                //Logger.ConsoleLog(client.WSData, ConsoleColor.Yellow);
                string[] wsdata = Regex.Split(client.WSData, BaseFuncs.WSplit);

                if (wsdata[0].Equals("GroupPage"))
                {
                    //ws.send("GroupPage{{=BaseFuncs.WSplit}}EntryToGroup{{=BaseFuncs.WSplit}}{{=Group.ID}}");
                    uint GroupID;
                    XUser User;

                    if (uint.TryParse(wsdata[2], out GroupID))
                    {
                        if (wsdata[1].Equals("Connect"))
                        {

                            if (WebSocketPage.ClientsGroupPage.ContainsKey(GroupID))
                            {
                                WebSocketPage.ClientsGroupPage[GroupID].Add(client);
                            }
                            else
                            {
                                List<Client> cls = new List<Client>();
                                cls.Add(client);
                                WebSocketPage.ClientsGroupPage.Add(GroupID, cls);
                            }
                        }
                        else if (wsdata[1].Equals("EntryToGroup") && Helper.UserHelper.GetCurrentUser(client, out User))
                        {
                            int OldGroup = User.GroupOwnerID;
                            if (Helper.GroupHelper.EnterToGroup(GroupID, client))
                            {
                                WebSocketPage.UpdateGroupData(GroupID);
                                if (OldGroup > -1)
                                {
                                    WebSocketPage.UpdateGroupData(Convert.ToUInt32(OldGroup));
                                }
                            }
                        }
                    }
                }
                else if(wsdata[0].Equals("MyGroupPage"))
                {
                    XUser User;
                    if (Helper.UserHelper.GetCurrentUser(client, out User))
                    {
                        if (wsdata[1].Equals("ChangeName"))
                        {
                            if (Helper.GroupHelper.UpdateNameByID(User.ID, wsdata[2]))
                            {
                                WebSocketPage.UpdateGroupData(User.ID);
                            }
                        }
                    }
                }
                else if (wsdata[0].Equals("GetInventory"))
                {
                    XUser User;
                    uint SteamGameID;
                    if (Helper.UserHelper.GetCurrentUser(client, out User)&& uint.TryParse(wsdata[1], out SteamGameID))
                    {
                        UsersInventory Inventory = null;
                        bool InList = Helper.UserHelper.GetSteamInventory(User, SteamGameID, out Inventory);
                        if (InList)
                        {
                            if (!Inventory.Opened)
                            {
                                WebSocketPage.UpdateInventory("", 0d, client, false);
                            }
                            else
                            {
                                string StrItems = "";
                                foreach (USteamItem Item in Inventory.SteamItems)
                                {
                                    StrItems += WebSocketPage.InventoryItemToString(Item);
                                }

                                WebSocketPage.UpdateInventory(StrItems, Inventory.TotalPrice, client, true);
                            }
                        }
                        else
                        {
                            Helper.UserHelper.WaitingList_InventoryClient(User.ID, client, SteamGameID);
                        }
                    }
                }
            }

            return false;
        }

        public static void UpdateGroupData(uint GroupID)
        {
            UGroup group;
            if (Helper.GroupHelper.SelectByID(GroupID, out group) && WebSocketPage.ClientsGroupPage.ContainsKey(group.ID))
            {
                for (int i = 0; i < WebSocketPage.ClientsGroupPage[group.ID].Count; i++)
                {
                    if (WebSocketPage.ClientsGroupPage[group.ID][i].Closed)
                    {
                        WebSocketPage.ClientsGroupPage[group.ID].Remove(WebSocketPage.ClientsGroupPage[group.ID][i]);
                        continue;
                    }

                    XUser user;
                    int UsersGroupID = -1;
                    if (Helper.UserHelper.GetCurrentUser(WebSocketPage.ClientsGroupPage[group.ID][i], out user))
                    {
                        UsersGroupID = user.GroupOwnerID;
                    }
                    // 0: action, 1:name, 2: usercouent, 3: group_id, 4: my_group_id
                    WebSocketPage.ClientsGroupPage[group.ID][i].SendWebsocket("UpdateGroupData" + BaseFuncs.WSplit + group.Name + BaseFuncs.WSplit + group.UserCount + BaseFuncs.WSplit + group.ID + BaseFuncs.WSplit + UsersGroupID);
                }
            }
        }

        public static void UpdateInventory(string StrItems, double TotalPrice, Client client, bool InventoryOpened)
        {
            if (InventoryOpened)
            {
                client.SendWebsocket("Inventory" + BaseFuncs.WSplit + TotalPrice + BaseFuncs.WSplit + StrItems);
                return;
            }
            client.SendWebsocket("InventoryClosed" + BaseFuncs.WSplit);
        }

        public static string InventoryItemToString(USteamItem SteamItem)
        {
            // 4- last
            return SteamItem.Name + "↓" + SteamItem.Price + "↓" + SteamItem.SteamGameID + "↓" + SteamItem.ID + "↓" + SteamItem.AssertID + ";";
        }

        public static void ChangeBetProcessStatus(ulong UserSteamID, ushort status)
        {
            Logger.ConsoleLog("Send WS TO USER!! status: " + status, ConsoleColor.Yellow);
        }
    }
}
