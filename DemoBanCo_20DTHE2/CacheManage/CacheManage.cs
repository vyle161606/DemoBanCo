using Libs.Entity;
using Libs.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace DemoBanCo_20DTHE2.CacheManage
{
    public class CacheManage
    {
        private ChessService chessService;
        private IMemoryCache memoryCache;

        public CacheManage(ChessService chessService, IMemoryCache memoryCache ) {
            this.chessService = chessService;
            this.memoryCache = memoryCache;
        }
        public Dictionary<string, List<UserInRoom>> UserInRoom
        {
            get {
                Dictionary<string, List<UserInRoom>> userInRoomDic = (Dictionary<string, List<UserInRoom>>) memoryCache.Get("memoryCache");
                if (userInRoomDic == null) {
                    userInRoomDic = new Dictionary<string, List<UserInRoom>>();
                    List<UserInRoom> userInRoomList = chessService.getUserInRoomList();
                    for(int i = 0; i< userInRoomList.Count; i++)
                    {
                        if (!userInRoomDic.ContainsKey(userInRoomList[i].RoomId.ToString().ToLower()))
                        {
                            List<UserInRoom> userInRoomTemp = new List<UserInRoom>();
                            userInRoomTemp.Add(userInRoomList[i]);
                            userInRoomDic.Add(userInRoomList[i].RoomId.ToString().ToLower(), userInRoomTemp);
                        }
                        else {
                            List<UserInRoom> userInRoomTemp = userInRoomDic[userInRoomList[i].RoomId.ToString().ToLower()];
                            userInRoomTemp.Add(userInRoomList[i]);
                        }
                    }
                    memoryCache.Set("memoryCache", userInRoomDic);
                }
                return userInRoomDic;
            }
        }
    }
}
