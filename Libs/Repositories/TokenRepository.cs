using Libs.Data;
using Libs.DTOs;
using Libs.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libs.Repositories
{
    public interface ITokenRepository : IRepository<RefreshToken>
    {
        Task AddTokenAsync(RefreshToken token);
        Task SaveTokenAsync();
        void UpdateToken(RefreshToken token);
        Task<RefreshToken> StoredToken(TokenRequest tokenRequest);
    }
    public class TokenRepository : RepositoryBase<RefreshToken>, ITokenRepository
    {
        public TokenRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task AddTokenAsync(RefreshToken token)
        {
            await _dbContext.RefreshTokens.AddAsync(token);
        }
        public async Task SaveTokenAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
        public void UpdateToken(RefreshToken token)
        {
            _dbContext.RefreshTokens.Update(token);
        }
        public async Task<RefreshToken> StoredToken(TokenRequest tokenRequest)
        {
            return await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
        }

    }
}
