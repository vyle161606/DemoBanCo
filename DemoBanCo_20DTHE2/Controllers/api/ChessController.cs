using DemoBanCo_20DTHE2.Hubs;
using DemoBanCo_20DTHE2.Models;
using Libs.Entity;
using Libs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace DemoBanCo_20DTHE2.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChessController : ControllerBase
    {
        private ChessService chessService;
        private IMemoryCache memoryCache;
        private IWebHostEnvironment hostEnvironment;
        private IHubContext<ChatHub> hubContext;
        private CacheManage.CacheManage cachesManage;

        public ChessController(ChessService chessService, IMemoryCache memoryCache, IWebHostEnvironment hostEnvironment, IHubContext<ChatHub> hubContext)
        {
            this.chessService = chessService;
            this.memoryCache = memoryCache;
            this.hostEnvironment = hostEnvironment;
            this.hubContext = hubContext;
            this.cachesManage = new CacheManage.CacheManage(this.chessService, this.memoryCache);
        }
        
        [HttpPost]
        [Route("insertRoom")]
        public IActionResult insertRoom(string roomName)
        {
            Room room = new Room();
            room.Name = roomName;
            room.Id = Guid.NewGuid();
            chessService.insertRoom(room);
            return Ok(new { status = true, message = "" });
        }
        [HttpGet]
        [Route("getRoom")]
        public IActionResult getRoom()
        {
            List<Room> roomList = chessService.getRoomList();
            return Ok(new { status = true, message = "", data = roomList });
        }
        [HttpPost]
        [Route("addUserToRoom")]
        public IActionResult addUserToRoom(Guid roomId, string userName) //user will get from entity sercurity
        {
            UserInRoom usInRoom = new UserInRoom();
            usInRoom.Id = Guid.NewGuid();
            usInRoom.RoomId = roomId;
            usInRoom.UserName = userName;

            chessService.insertUserInRoom(usInRoom);

            if (!cachesManage.UserInRoom.ContainsKey(roomId.ToString().ToLower()))
            {
                List<UserInRoom> userInRoomTemp = new List<UserInRoom>();
                userInRoomTemp.Add(usInRoom);
                cachesManage.UserInRoom.Add(roomId.ToString().ToLower(), userInRoomTemp);
            }
            else
            {
                List<UserInRoom> userInRoomTemp = cachesManage.UserInRoom[roomId.ToString().ToLower()];
                userInRoomTemp.Add(usInRoom);
            }
            return Ok(new { status = true, message = "" });
        }
        [HttpGet]
        [Route("getUserInRoom")]
        public IActionResult getUserInRoom(Guid roomId)
        {
            List<UserInRoom> userInRoomList = cachesManage.UserInRoom[roomId.ToString().ToLower()];// chessService.getUserInRoomList(roomId);
            return Ok(new { status = true, message = "", data = userInRoomList });
        }
        [HttpGet]
        [Route("loadchessboard")]
        public IActionResult chessBoard()
        {
            string chessJson = System.IO.File.ReadAllText(hostEnvironment.ContentRootPath + "\\Data\\ChessJson.txt");
            List<ChessNode> chessNodeList = JsonSerializer.Deserialize<List<ChessNode>>(chessJson);
            List<List<PointModel>> matrix = new List<List<PointModel>>();
            for (int i = 0; i < 10; i++)
            {
                int top = 61 + i * 74;
                List<PointModel> pointList = new List<PointModel>();
                for (int j = 0; j < 9; j++)
                {
                    int left = 106 + j * 74;
                    PointModel p = new PointModel();
                    p.top = top;
                    p.left = left;
                    p.id = "";
                    ChessNode chessNode = chessNodeList.Where(s => s.top == top && s.left == left).FirstOrDefault();
                    if (chessNode != null)
                        p.id = chessNode.id;
                    pointList.Add(p);
                }
                matrix.Add(pointList);
            }
            return Ok(new { status = true, message = "", matrix = matrix, chessNode = chessNodeList });
        }
        [HttpPost]
        [Route("movechess")]
        public IActionResult moveChess(List<MoveChess> moveNodeList)
        {
            hubContext.Clients.All.SendAsync("ReceiveChessMove", JsonSerializer.Serialize(moveNodeList));
            return Ok(new { status = true, message = "" });
        }
        [HttpGet("userinfo")]
        public IActionResult GetUserInfo()
        {
            // Lấy thông tin người dùng từ Claims
            var userId = User.FindFirst("Id")?.Value;
            var username = User.FindFirst("Username")?.Value;

            return Ok(new
            {
                UserId = userId,
                Username = username
            });
        }

    }
}
