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
                UFile image;
                if(Helper.SteamItemsHelper.GetImageFromMemory(ItemID, SteamGameID, out image))
                {
                    CachedFile cf = new CachedFile();
                    cf.Data = image.Data;
                    cf.CacheKey = BaseFuncs.MD5(BaseFuncs.MD5(cf.Data) + BaseFuncs.MD5(image.Name));
                    cf.Header = "HTTP/1.1 200\nCache-Control: public, max-age=60000\nETag: " + cf.CacheKey + "\nServer: UpServer\nContent-Type: " + FileSender.GetContentType(Configs.STEAM_ITEMS_TYPE) + "; charset=UTF-8|!cookie!|\nConnection: keep-alive\nContent-Length: |!btsize!|\n\n";
                    FileSender.SendCachedFile(client, cf);
                    //FileSender.SendUserFile(client, image);
                }
            }

            BaseFuncs.Show404(client);
            return false;
        }
    }
}
