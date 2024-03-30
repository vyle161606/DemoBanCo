using Libs.Data;
using Libs.Entity;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libs.Repositories
{
    public interface  IRoomRepository: IRepository<Room>
    {
        public void insertRoom(Room room);
        public List<Room> getRoomList();
    }
    public class RoomRepository : RepositoryBase<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
        public void insertRoom(Room room) { 
            _dbContext.Room.Add(room);
        }
        public List<Room> getRoomList() { 
            return _dbContext.Room.ToList();
        }
    }
}
