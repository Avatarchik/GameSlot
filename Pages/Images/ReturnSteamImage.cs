using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Images
{
    public class ReturnSteamImage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/steam-image/"; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool Init(Client client)
        {
            uint SteamGameID, ItemID;
            if (uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0], out SteamGameID) && uint.TryParse(BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[1], out ItemID))
            {
                CachedFile image;
                if(Helper.SteamItemsHelper.GetImageFromMemory(ItemID, SteamGameID, out image))
                {
                    FileSender.SendCachedFile(client, image);
                    //FileSender.SendUserFile(client, image);
                }
            }

            BaseFuncs.Show404(client);
            return false;
        }
    }
}
