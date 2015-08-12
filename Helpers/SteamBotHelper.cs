using GameSlot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpServer;
using XData;

namespace GameSlot.Helpers
{
    public class SteamBotHelper
    {
        public XTable<XSteamBot> Table = new XTable<XSteamBot>();
        public XTable<XSteamBotProcessItem> Table_Items = new XTable<XSteamBotProcessItem>();

        public SteamBotHelper()
        {
            new Thread(delegate()
            {
                try
                {
                    new UTSteam();
                }
                catch (Exception ex)
                {
                    Logger.ConsoleLog(ex);
                }
            }).Start();
            
        }

        public bool GetFreeBot(out XSteamBot XSteamBot)
        {
            XSteamBot = new XSteamBot();
            List<XSteamBot> steambots = this.Table.SelectAll();

            if (steambots.Count > 0)
            {
                int count = 0;
                foreach (XSteamBot Bot in steambots)
                {
                    List<XSteamBotProcessItem> processes;

                    if (this.Table_Items.Select(data => data.SteamBotID == Bot.ID && data.Status > 0, out processes))
                    {
                        if (processes.Count > count)
                        {
                            count = processes.Count;
                            XSteamBot = Bot;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public bool SelectByOfferID(ulong OfferID, ulong UserSteamID, out XSteamBotProcessItem XSteamBotProcessItem)
        {
            XSteamBotProcessItem = new XSteamBotProcessItem();
            return this.Table_Items.SelectOne(data => data.UserSteamID == UserSteamID && data.OfferID == OfferID, out XSteamBotProcessItem);
        }
    }
}
