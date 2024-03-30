using Libs.Entity;
using Libs.Repositories;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libs.Services
{
    public class ChessService
    {
        private ApplicationDbContext applicationDbContext;
        private IRoomRepository roomRepository;
        private IUserInRoomRepository userInRoomRepository;

        public ChessService(ApplicationDbContext applicationDbContext) {
            this.applicationDbContext = applicationDbContext;
            this.roomRepository = new RoomRepository(applicationDbContext);
            this.userInRoomRepository = new UserInRoomRepository(applicationDbContext);

        }
        public void Save() { 
            applicationDbContext.SaveChanges();
        }
        public void insertRoom(Room room) { 
            roomRepository.insertRoom(room);
            Save();
        }
        public List<Room> getRoomList() { 
            return roomRepository.getRoomList();
        }
        public void insertUserInRoom(UserInRoom usInRoom)
        {
            userInRoomRepository.insertUserInRoom(usInRoom);
            Save();
        }
        public List<UserInRoom> getUserInRoomList(Guid roomId)
        {
            return userInRoomRepository.getUserInRoomList(roomId);
        }
        public List<UserInRoom> getUserInRoomList()
        {
            return userInRoomRepository.getUserInRoomList();
        }
    }
}
