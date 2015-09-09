using GameSlot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.UserPages
{
    public class PaymentPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/payment/do"; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            XUser User;
            if (Helper.UserHelper.GetCurrentUser(client, out User))
            {
                double price;
                if(double.TryParse(client.PostParam("price"), out price))
                {
                    price = Math.Round(price);

                    XOrder XOrder = new XOrder();
                    XOrder.UserID = User.ID;
                    XOrder.Status = 0;
                    XOrder.Price = price;
                    uint order_id = Helper.OrderHelper.Table.Insert(XOrder);

                    client.Redirect(Robokassa.GetPaymentURL(Helper.OrderHelper.Table.SelectByID(order_id)));
                    return false;
                }
                else
                {
                    BaseFuncs.Show404(client);
                    return false;
                }
            }

            BaseFuncs.Show403(client);
            return false;
        }
    }
}
