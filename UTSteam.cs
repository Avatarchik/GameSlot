using GameSlot.Database;
using GameSlot.Pages.Includes;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpServer;
using UpTunnel;

namespace GameSlot
{
    class UTSteam
    {
        public static Dictionary<string, string> Offers = new Dictionary<string, string>();
        public static Socket sk = null;

        public static Dictionary<ulong, Client> ClientsOffer = new Dictionary<ulong, Client>();
        public static Dictionary<ulong, Client> SendClientOffer = new Dictionary<ulong, Client>();

        public UTSteam()
        {
            UTClient uc = new UTClient("localhost", 7712, "GameSlotTestes", data =>
            {
                if (data.GetType() == typeof(string))
                {
                    string val = (string)data;
                    if (val.Contains(':'))
                    {
                        string[] args = val.Split(':');

                        /*for (uint i = 0; i < args.Length; i++)
                        {
                            Logger.ConsoleLog(args[i], ConsoleColor.Green);
                        }*/

                        if (args[0] == "accepted")//чел принял
                        {
                            Logger.ConsoleLog("User accepted!", ConsoleColor.Green);
                            XBotsOffer XBotsOffer;
                            XSteamBotProcessItems XSteamBotProcessItem;
                            if (Helper.SteamBotHelper.SelectByOfferID(Convert.ToUInt64(args[2]), out XSteamBotProcessItem))
                            {
                                XSteamBotProcessItem.Status = 2;
                                XSteamBotProcessItem.StatusChangedTime = Helper.GetCurrentTime();                         
                                Helper.SteamBotHelper.Table_Items.UpdateByID(XSteamBotProcessItem, XSteamBotProcessItem.ID);

                                //Logger.ConsoleLog(XSteamBotProcessItem.SteamItemsNum + "::", ConsoleColor.Green);

                                for(ushort i = 0; i < XSteamBotProcessItem.SteamItemsNum; i++)
                                {
                                    XSItemUsersInventory XSItemUsersInventory = new XSItemUsersInventory();
                                    XSItemUsersInventory.UserID = XSteamBotProcessItem.UserID;
                                    XSItemUsersInventory.SteamItemID = XSteamBotProcessItem.SteamItemIDs[i];
                                    XSItemUsersInventory.AssertID = XSteamBotProcessItem.ItemAssertIDs[i];
                                    XSItemUsersInventory.SteamGameID = XSteamBotProcessItem.SteamGameID;
                                    XSItemUsersInventory.SteamBotID = XSteamBotProcessItem.SteamBotID;

                                    Helper.UserHelper.Table_SteamItemUsersInventory.Insert(XSItemUsersInventory);
                                    Helper.UserHelper.DeleteItemFromSteamInventory(XSItemUsersInventory.UserID, XSItemUsersInventory.AssertID, XSItemUsersInventory.SteamGameID);
                                }

                                ProcessingBet ProcessingBet;
                                if (Helper.LotteryHelper.SelectProcessingBet(XSteamBotProcessItem.UserID, XSteamBotProcessItem.SteamGameID, out ProcessingBet))
                                {
                                    Helper.LotteryHelper.RemoveProcessingBet(ProcessingBet.UserID, ProcessingBet.SteamGameID);
                                    XLottery xlottery;
                                    if (Helper.LotteryHelper.Table.SelectByID(ProcessingBet.LotteryID, out xlottery) && xlottery.WinnersToken == 0)
                                    {
                                        Logger.ConsoleLog("Prosessing steam items:");
                                        foreach(USteamItem Item in ProcessingBet.SteamItems)
                                        {
                                            Logger.ConsoleLog("Assert ID:" + Item.AssertID);
                                        }

                                        ushort result = Helper.LotteryHelper.SetBet(xlottery.ID, ProcessingBet.UserID, ProcessingBet.SteamItems, ProcessingBet.Chips, ProcessingBet.client);

                                        if (result != 2)
                                        {
                                            ProcessingBet.client.SendWebsocket("BetDone" + BaseFuncs.WSplit + result);
                                        }
                                    }
                                }
                            }

                            else if (Helper.SteamBotHelper.Table_BotsOffer.SelectOne(bt => bt.OfferID == Convert.ToUInt64(args[2]), out XBotsOffer))
                            {
                                XBotsOffer.Status = 4;
                                Helper.SteamBotHelper.Table_BotsOffer.UpdateByID(XBotsOffer, XBotsOffer.ID);
                            }

                            Logger.ConsoleLog("User with steamid: '" + args[1] + "' applied tradeoffer with ID: " + args[2]);
                            Offers.Remove(args[2]);
                        }
                        else if (args[0] == "declined")//чел отменил
                        {
                            Helper.LotteryHelper.CancelBet(Convert.ToUInt64(args[2]), Convert.ToUInt64(args[1]), 4);

                            XBotsOffer XBotsOffer;
                            if (Helper.SteamBotHelper.Table_BotsOffer.SelectOne(bt => bt.OfferID == Convert.ToUInt64(args[2]), out XBotsOffer))
                            {
                                XBotsOffer.Status = 2;
                                Helper.SteamBotHelper.Table_BotsOffer.UpdateByID(XBotsOffer, XBotsOffer.ID);
                                Helper.SteamBotHelper.DeclinedLocalItemOffer(XBotsOffer.OfferID);

                                WebSocketPage.SendItemsOffer(XBotsOffer.SteamUserID, 2, XBotsOffer.OfferID);
                            }

                            Logger.ConsoleLog("User with steamid: '" + args[1] + "' declined tradeoffer with ID: " + args[2]);
                            Offers.Remove(args[2]);
                        }
                        else if (args[0] == "state_unknown")//предложение потерялось (статус неизвестен)
                        {
                            Helper.LotteryHelper.CancelBet(Convert.ToUInt64(args[2]), Convert.ToUInt64(args[1]), 6);

                            //Logger.ConsoleLog("User with steamid: '" + args[1] + "' tradeoffer with ID: " + args[2] + " lost!");
                            Offers.Remove(args[2]);
                        }
                        else if (args[0] == "no_offer")//предложения нет(ответ на попытку отменить)
                        {
                            XBotsOffer XBotsOffer;
                            XSteamBotProcessItems Process;
                            if (Helper.SteamBotHelper.SelectByOfferID(ulong.Parse(args[1]), out Process))
                            {
                                Process.Status = 7;
                                Process.StatusChangedTime = Helper.GetCurrentTime();
                                Helper.SteamBotHelper.Table_Items.UpdateByID(Process, Process.ID);
                            }

                            else if (Helper.SteamBotHelper.Table_BotsOffer.SelectOne(bt => bt.OfferID == Convert.ToUInt64(args[1]), out XBotsOffer))
                            {
                                XBotsOffer.Status = 6;
                                Helper.SteamBotHelper.Table_BotsOffer.UpdateByID(XBotsOffer, XBotsOffer.ID);
                            }

                            Logger.ConsoleLog("'No offer with that id(may be already processed) ID::: " + args[1]);
                            Offers.Remove(args[1]);
                        }
                        else if (args[0] == "declined_by_system")//отмена успешна
                        {
                            XBotsOffer XBotsOffer;
                            XSteamBotProcessItems Process;
                            if (Helper.SteamBotHelper.SelectByOfferID(ulong.Parse(args[1]), out Process))
                            {
                                Process.Status = 6;
                                Process.StatusChangedTime = Helper.GetCurrentTime();
                                Helper.SteamBotHelper.Table_Items.UpdateByID(Process, Process.ID);
                            }

                            else if (Helper.SteamBotHelper.Table_BotsOffer.SelectOne(bt => bt.OfferID == Convert.ToUInt64(args[1]), out XBotsOffer))
                            {
                                XBotsOffer.Status = 3;
                                Helper.SteamBotHelper.Table_BotsOffer.UpdateByID(XBotsOffer, XBotsOffer.ID);
                                Helper.SteamBotHelper.DeclinedLocalItemOffer(XBotsOffer.OfferID);
                            }

                            Logger.ConsoleLog("'Offer declined successfully ID::: " + args[1]);
                            Offers.Remove(args[1]);
                        }
                        else if (args[0] == "sent_offer") // запрос шмоток
                        {                           
                            XSteamBotProcessItems XSteamBotProcessItem;
                            if (Helper.SteamBotHelper.Table_Items.SelectOne(bt => bt.UserSteamID == Convert.ToUInt64(args[1]) && bt.Status == 0, out XSteamBotProcessItem))
                            {
                                XSteamBotProcessItem.Status = 1;
                                XSteamBotProcessItem.SentTime = Helper.GetCurrentTime();
                                XSteamBotProcessItem.OfferID = Convert.ToUInt64(args[2]);
                                // TODO: отправит уведомление, что выслано
                                Helper.SteamBotHelper.Table_Items.UpdateByID(XSteamBotProcessItem, XSteamBotProcessItem.ID);

                                if(UTSteam.ClientsOffer.ContainsKey(XSteamBotProcessItem.UserSteamID))
                                {
                                    ulong steamID = XSteamBotProcessItem.UserSteamID;
                                    UTSteam.ClientsOffer[steamID].SendWebsocket("BetDone" + BaseFuncs.WSplit + "2" + BaseFuncs.WSplit + args[2] + BaseFuncs.WSplit
                                        + XSteamBotProcessItem.ProtectionCode + BaseFuncs.WSplit + Helper.SteamBotHelper.Table.SelectByID(XSteamBotProcessItem.SteamBotID).Name);
                                }
                            }

                            Logger.ConsoleLog("Offer sent to steamid: '" + args[1] + "'; Got offer ID: " + args[2]);
                            Offers.Add(args[2], args[1]);
                        }
                        else if (args[0] == "sending_error") // ошибка запроса шмоток(стим ответил ошибкой 500), скорее всего такого итема у чула нет(например передал кому то со времени последнего апдейта инвентаря)
                        {
                            XBotsOffer XBotsOffer;
                            if (Helper.SteamBotHelper.Table_BotsOffer.SelectOne(bt => bt.SteamUserID == Convert.ToUInt64(args[1]) && bt.Status == 0, out XBotsOffer))
                            {
                                XBotsOffer.Status = 5;
                                Helper.SteamBotHelper.Table_BotsOffer.UpdateByID(XBotsOffer, XBotsOffer.ID);
                                Helper.SteamBotHelper.ErrorToSendLocalItem(XBotsOffer.SteamUserID, XBotsOffer.SentTime);

                                WebSocketPage.SendItemsOffer(XBotsOffer.SteamUserID, 0);
                            }

                            Logger.ConsoleLog("Cant send offer to steamid: '" + args[1] + "'; error occured ");
                            Offers.Add(args[2], args[1]);
                        }
                        else if (args[0] == "sent_items")// отправка приза
                        {
                            XBotsOffer XBotsOffer;
                            if (Helper.SteamBotHelper.Table_BotsOffer.SelectOne(bt => bt.SteamUserID == Convert.ToUInt64(args[1]) && bt.Status == 0, out XBotsOffer))
                            {
                                XBotsOffer.Status = 1;
                                XBotsOffer.OfferID = Convert.ToUInt64(args[2]);
                                Helper.SteamBotHelper.Table_BotsOffer.UpdateByID(XBotsOffer, XBotsOffer.ID);

                                WebSocketPage.SendItemsOffer(XBotsOffer.SteamUserID, 1, XBotsOffer.OfferID);
                                Logger.ConsoleLog("Updated XBotsOffer status to " + XBotsOffer.Status);
                            }

                            Logger.ConsoleLog("Items sent to steamid: '" + args[1] + "' offer.ID: " + args[2]);
                        }
                        else
                        {
                            Logger.ConsoleLog("Unknown command");
                        }

                    }
                }
                return true;
            });
            sk = uc.client.Client;


            if(uc.Connected)
            {
                Logger.ConsoleLog("UTClient connected!", ConsoleColor.Yellow);

                new Thread(delegate()
                {
                    while (true)
                    {
                        XSteamBotProcessItems[] XSteamBotProcessItems;
                        if (Helper.SteamBotHelper.Table_Items.SelectArr(data => data.Status == 1 && (data.SentTime + 360 < Helper.GetCurrentTime()), out XSteamBotProcessItems))
                        {
                            for (int i = 0; i < XSteamBotProcessItems.Length; i++)
                            {
                                UpTunnel.Sender.Send(UTSteam.sk, "decline:" + XSteamBotProcessItems[i].OfferID);
                            }
                        }

                        Thread.Sleep(1000);
                    }
                }).Start();


                new Thread(delegate()
                {
                    while (true)
                    {
                        XBotsOffer[] XBotsOffer;
                        if (Helper.SteamBotHelper.Table_BotsOffer.SelectArr(data => data.Status <= 1 && (data.SentTime + 3660 + 120 < Helper.GetCurrentTime()), out XBotsOffer))
                        {
                            for (int i = 0; i < XBotsOffer.Length; i++)
                            {
                                if (XBotsOffer[i].OfferID == 0)
                                {
                                    Logger.ConsoleLog("Trying to decline offer with 0 offer_id status:" + XBotsOffer[i].Status + " UserID:" + XBotsOffer[i].UserID
                                        + " SteamUserID:" + XBotsOffer[i].SteamUserID, ConsoleColor.Yellow, LogLevel.Error);

                                    XBotsOffer XBotsOffer_fail = Helper.SteamBotHelper.Table_BotsOffer.SelectByID(XBotsOffer[i].ID);
                                    XBotsOffer_fail.Status = 5;
                                    Helper.SteamBotHelper.Table_BotsOffer.UpdateByID(XBotsOffer_fail, XBotsOffer_fail.ID);

                                    continue;
                                }

                                UpTunnel.Sender.Send(UTSteam.sk, "decline:" + XBotsOffer[i].OfferID);
                            }
                        }

                        Thread.Sleep(1000);
                    }
                }).Start();  

            }
        }
    }
}
