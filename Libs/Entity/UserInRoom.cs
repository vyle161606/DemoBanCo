using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libs.Entity
{
    public class UserInRoom
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string UserName { get; set; }

    }
}
