using GameSlot.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class ProfPage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Maintenance; }
        }
        public override bool Init(Client client)
        {
            Hashtable data = new Hashtable();
            data.Add("Title", "Идет профилактика");
            client.HttpSend("Сайт на профилактике. <br />Пока вы можете посетить наше ВК сообщество: <a href='https://vk.com/thegameslot'>https://vk.com/thegameslot</a>");
            return true;
        }
    }
}
