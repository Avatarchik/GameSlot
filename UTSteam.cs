using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UpServer;
using UpTunnel;

namespace GameSlot
{
    class UTSteam
    {
        public static Dictionary<string, string> Offers = new Dictionary<string, string>();
        public static Socket sk = null;
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

                        for (uint i = 0; i < args.Length; i++)
                        {
                            Logger.ConsoleLog(args[i], ConsoleColor.Green);
                        }

                        if (args[0] == "accepted")//чел принял
                        {
                            XSteamBotProcessItem XSteamBotProcessItem;
                            if (Helper.SteamBotHelper.SelectByOfferID(Convert.ToUInt64(args[2]), Convert.ToUInt64(args[1]), out XSteamBotProcessItem))
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
                                }

                                ProcessingBet ProcessingBet;
                                if (Helper.LotteryHelper.SelectProcessingBet(XSteamBotProcessItem.UserID, XSteamBotProcessItem.SteamGameID, out ProcessingBet))
                                {
                                    Logger.ConsoleLog("process can be used!!", ConsoleColor.Green);
                                    Helper.LotteryHelper.RemoveProcessingBet(ProcessingBet);
                                    Logger.ConsoleLog(Helper.LotteryHelper.SetBet(Helper.LotteryHelper.GetCurrent(XSteamBotProcessItem.SteamGameID).ID, XSteamBotProcessItem.UserID, ProcessingBet.SteamItems, ProcessingBet.Chips), ConsoleColor.Yellow);
                                }
                            }

                            Logger.ConsoleLog("User with steamid: '" + args[1] + "' applied tradeoffer with ID: " + args[2]);
                            Offers.Remove(args[2]);
                        }
                        else if (args[0] == "declined")//чел отменил
                        {
                            Helper.LotteryHelper.CancelBet(Convert.ToUInt64(args[2]), Convert.ToUInt64(args[1]), 4);

                            Logger.ConsoleLog("User with steamid: '" + args[1] + "' declined tradeoffer with ID: " + args[2]);
                            Offers.Remove(args[2]);
                        }
                        else if (args[0] == "state_unknown")//предложение потерялось (статус неизвестен)
                        {
                            Helper.LotteryHelper.CancelBet(Convert.ToUInt64(args[2]), Convert.ToUInt64(args[1]), 6);

                            Logger.ConsoleLog("User with steamid: '" + args[1] + "' tradeoffer with ID: " + args[2] + " lost!");
                            Offers.Remove(args[2]);
                        }
                        else if (args[0] == "no_offer")//предложения нет(ответ на попытку отменить)
                        {
                            Logger.ConsoleLog("'No offer with that id(may be already processed) ID::: " + args[1]);
                            Offers.Remove(args[1]);
                        }
                        else if (args[0] == "declined_by_system")//отмена успешна
                        {
                            Logger.ConsoleLog("'Offer declined successfully ID::: " + args[1]);
                            Offers.Remove(args[1]);
                        }
                        else if (args[0] == "sent_offer") // запрос шмоток
                        {
                            XSteamBotProcessItem XSteamBotProcessItem;
                            if (Helper.SteamBotHelper.Table_Items.SelectOne(bt => bt.UserSteamID == Convert.ToUInt64(args[1]) && bt.Status == 0, out XSteamBotProcessItem))
                            {
                                XSteamBotProcessItem.Status = 1;
                                XSteamBotProcessItem.SentTime = Helper.GetCurrentTime();
                                XSteamBotProcessItem.OfferID = Convert.ToUInt64(args[2]);
                                // TODO: отправит уведомление, что выслано
                                Helper.SteamBotHelper.Table_Items.UpdateByID(XSteamBotProcessItem, XSteamBotProcessItem.ID);
                            }

                            Logger.ConsoleLog("Offer sent to steamid: '" + args[1] + "'; Got offer ID: " + args[2]);
                            Offers.Add(args[2], args[1]);
                        }
                        else if (args[0] == "sending_error") // ошибка запроса шмоток(стим ответил ошибкой 500), скорее всего такого итема у чула нет(например передал кому то со времени последнего апдейта инвентаря)
                        {
                            Logger.ConsoleLog("Cant send offer to steamid: '" + args[1] + "'; error occured ");
                            Offers.Add(args[2], args[1]);
                        }
                        else if (args[0] == "sent_items")// отправка приза
                        {
                            Logger.ConsoleLog("Items sent to steamid: '" + args[1] + "'");
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
        }
    }
}
