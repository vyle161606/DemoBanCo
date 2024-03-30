using Libs.DTOs;
using Libs.Entity;
using Libs.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libs.Services
{
    public class TokenService
    {
        private ITokenRepository tokenRepository;
        public TokenService(ApplicationDbContext applicationDbContext)
        {
            this.tokenRepository = new TokenRepository(applicationDbContext);
        }
        public async Task AddTokenAsync(RefreshToken token)
        {
            await tokenRepository.AddTokenAsync(token);
        }
        public async Task SaveTokenAsync()
        {
            await tokenRepository.SaveTokenAsync();
        }
        public void UpdateToken(RefreshToken token)
        {
            tokenRepository.UpdateToken(token);
        }
        public async Task<RefreshToken> StoredToken(TokenRequest tokenRequest)
        {
            return await tokenRepository.StoredToken(tokenRequest);
        }
    }
}
