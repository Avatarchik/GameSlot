using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot
{
    public class Configs
    {
        public static uint DOTA2_STEAM_GAME_ID = 570;
        public static uint CSGO_STEAM_GAME_ID = 730;

        public static string STEAM_API = "D2D57807EDF7C09134C7F1BA077A9658";

        // USD
        public static double MIN_ITEMS_PRICE = 0.1;

        // secs:
        public static int LOTTERY_GAME_TIME = 300;
        public static int LOTTERY_EXTRA_TIME = 180;
        public static int INVENTORY_UPDATE_TIME = 5;

        public static string STEAM_ITEMS_STORAGE = "SteamItems\\";
        public static string STEAM_IMAGE_TYPE = ".png";
        public static string STEAM_ITEMS_STORAGE_JS = "SteamItems/";

        public static double TOKEN_PRICE = 0.1;

        public static int LOTTERY_BREAK_TIME = 30;


        public static int GROUP_PERCENTAGE = 5;
        public static int GAMESLOT_PERCENTAGE = 10;


        public static uint ADMIN_ACCOUNT = 0;
    }
}
