using Libs.DTOs;
using Libs.Entity;
using Libs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DemoBanCo_20DTHE2.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private TokenService _tokenService;
        private readonly TokenValidationParameters _tokenValidationParameters;
        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration, TokenService tokenService,
            TokenValidationParameters tokenValidationParameters)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._tokenService = tokenService;
            this._tokenValidationParameters = tokenValidationParameters;
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO loginRequest)
        {
            if (ModelState.IsValid)
            {
                var checkUser = await _userManager.FindByNameAsync(loginRequest.Username);
                if (checkUser == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Error = new List<string>()
                        {
                            "Username does not exist"
                        },
                        Result = false
                    });
                }
                var isCorrect = await _userManager.CheckPasswordAsync(checkUser, loginRequest.Password);
                if (!isCorrect)
                {
                    return BadRequest(new AuthResult()
                    {
                        Error = new List<string>()
                         {
                             "Password Invalid"
                         },
                        Result = false
                    });
                }
                else
                {
                    var jwtToken = await GenerateJwtToken(checkUser);
                    return Ok(jwtToken);
                }
            }
            return BadRequest(new AuthResult()
            {
                Error = new List<string>()
                {
                     "Invalid Payload"
                },
                Result = false
            });
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO registerRequest)
        {
            if (ModelState.IsValid)
            {
                var checkUserName = await _userManager.FindByNameAsync(registerRequest.Username);
                if (checkUserName != null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Error = new List<string>()
                        {
                            "Username already exist"
                        }
                    });
                }
                var passwordValidator = new PasswordValidator<IdentityUser>();
                var result = await passwordValidator.ValidateAsync(_userManager, null, registerRequest.Password);

                if (result.Succeeded)
                {
                    var newUser = new IdentityUser()
                    {
                        UserName = registerRequest.Username
                    };
                    var isCreate = await _userManager.CreateAsync(newUser, registerRequest.Password);
                    if (isCreate.Succeeded)
                    {
                        // Generate the token
                        var token = await GenerateJwtToken(newUser);
                        return Ok(token);
                    }
                }
                else
                {
                    // Mật khẩu không đủ mạnh
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Error = result.Errors.Select(error => error.Description).ToList()
                    });
                }
            }
            return BadRequest(new AuthResult()
            {
                Error = new List<string>()
                {
                     "Invalid Payload"
                },
                Result = false
            });
        }
        private async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHanlder = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);
            //Token Description
            var tokenDescription = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim("Username", user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
                }),
                //Setting time JWT
                Expires = DateTime.Now.Add(TimeSpan.Parse(_configuration.GetSection("JwtConfig:ExpiryTimeFrame").Value)),
                //Expires = DateTime.Now.AddSeconds(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
            var token = jwtTokenHanlder.CreateToken(tokenDescription);
            var jwtToken = jwtTokenHanlder.WriteToken(token);
            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                Token = GenerateRefreshToken(),
                AddedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddHours(1),
                IsRevoked = false,
                IsUsed = false,
                UserId = user.Id
            };
            await _tokenService.AddTokenAsync(refreshToken);
            await _tokenService.SaveTokenAsync();
            return new AuthResult()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Result = true
            };
        }
        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                return Convert.ToBase64String(random);
            }
        }
        [HttpPost]
        [Route("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);
            var tokenValidationParameter = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false
            };
            try
            {
                //check 1: Token valid format
                var tokenInverification = jwtTokenHandler.ValidateToken(tokenRequest.Token,
                    tokenValidationParameter, out var validatedToken);

                //check 2: Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);
                    if (result == false)
                    {
                        return Ok(new AuthResult()
                        {
                            Error = new List<string>()
                            {
                                 "Invalid token"
                            },
                            Result = false
                        });
                    }
                }
                //check 3: Check Token expire?
                var utcExpiryDate = long.Parse(tokenInverification.Claims
                    .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);
                if (expiryDate > DateTime.Now)
                {
                    return Ok(new AuthResult()
                    {
                        Error = new List<string>()
                        {
                             "Expired token"
                        },
                        Result = false
                    });
                }
                //check 4: Check refreshtoken exist in DB
                var storedToken = await _tokenService.StoredToken(tokenRequest);
                if (storedToken == null)
                {
                    return Ok(new AuthResult
                    {
                        Result = false,
                        Error = new List<string>()
                        {
                             "Refresh token does not exist"
                        }
                    });
                }

                //check 5: check refreshToken is used/revoked?
                if (storedToken.IsUsed)
                {
                    return Ok(new AuthResult
                    {
                        Result = false,
                        Error = new List<string>()
                        {
                             "Refresh token has been used"
                        }
                    });
                }
                if (storedToken.IsRevoked)
                {
                    return Ok(new AuthResult
                    {
                        Result = false,
                        Error = new List<string>()
                        {
                             "Refresh token has been revoked"
                        }
                    });
                }
                //check 6: Token id == JwtId in RefreshToken
                var jti = tokenInverification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return Ok(new AuthResult
                    {
                        Result = false,
                        Error = new List<string>()
                        {
                              "Token doesn't match"
                        }
                    });
                }
                //Update token is used
                storedToken.IsUsed = true;
                storedToken.IsRevoked = true;

                _tokenService.UpdateToken(storedToken);
                await _tokenService.SaveTokenAsync();
                var user = await _userManager.FindByIdAsync(storedToken.UserId);
                var token = await GenerateJwtToken(user);

                return Ok(new AuthResult
                {
                    Result = true,
                    Token = token.Token,
                    RefreshToken = token.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResult()
                {
                    Error = new List<string>()
                        {
                             "Server Error"
                        },
                    Result = false
                });
            }
        }
        
        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTimeVal;
        }
        
    }
}
