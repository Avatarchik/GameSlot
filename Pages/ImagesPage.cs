using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    class ImagesPage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/images/"; }
        }

        public override bool Init(Client client)
        {
            //Logger.ConsoleLog(BaseFuncs.GetAdditionalURL(client.URL, this.URL));
            FileSender.SendUserFile(client, BaseFuncs.GetAdditionalURL(client.URL, this.URL));
            return true;
        }
    }
}
