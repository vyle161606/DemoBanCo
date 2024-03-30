using Libs.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Libs
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Room>   Room { get; set; }
        public DbSet<UserInRoom> UserInRoom { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}