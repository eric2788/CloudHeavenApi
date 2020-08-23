using System;
using System.Threading.Tasks;
using CloudHeavenApi.Contexts;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudHeavenApi.Controllers
{
    [EnableCors]
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        private readonly ICacheService<Identity> _cacheService;

        private readonly HeavenContext _context;

        public AuthController(HeavenContext context, IAuthService authService, ICacheService<Identity> cacheService)
        {
            _context = context;
            _authService = authService;
            _cacheService = cacheService;
        }

        [HttpPost("validate")]
        public async Task<ActionResult> Validate([FromBody] AuthorizeRequest request)
        {
            var profile = await _authService.Refresh(request);
            var ac = await _context.WebAccounts.AsNoTracking().FirstOrDefaultAsync(account => account.Uuid == profile.UUID);
            if (ac == null)
            {
                await _authService.Invalidate(new AuthorizeRequest
                {
                    accessToken = profile.AccessToken,
                    clientToken = profile.ClientToken
                });
                throw new AuthException(new ErrorResponse
                {
                    Error = "未知的賬戶",
                    ErrorMessage = "你從未到伺服器登入過。"
                });
            }

            var user = new User(ac);

            return Ok(new
            {
                user,
                token = new
                {
                    clientToken = profile.ClientToken,
                    accessToken = profile.AccessToken
                }
            });
        }


        [HttpPost("authenticate")]
        public async Task<ActionResult> Authenticate([FromBody] AuthenticateRequest request)
        {
            var tokenProfile = await _authService.Authenticate(request);


            var ac = await _context.WebAccounts.AsNoTracking().FirstOrDefaultAsync(account => account.Uuid == tokenProfile.UUID);
            if (ac == null)
            {
                await _authService.Invalidate(new AuthorizeRequest
                {
                    accessToken = tokenProfile.AccessToken,
                    clientToken = tokenProfile.ClientToken
                });
                throw new AuthException(new ErrorResponse
                {
                    Error = "未知的賬戶",
                    ErrorMessage = "你從未到伺服器登入過。"
                });
            }

            var result = new
            {
                clientToken = tokenProfile.ClientToken,
                accessToken = tokenProfile.AccessToken
            };

            _cacheService.TryUpdate(tokenProfile.ClientToken, id => { id.NickName = ac.NickName; });

            if (ac.UserName != tokenProfile.UserName)
            {
                ac.UserName = tokenProfile.UserName;
                _context.Entry(ac).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }

            return Ok(result);
        }

        [HttpPost("signout")]
        public async Task<ActionResult> SignOut([FromBody] AuthorizeRequest request)
        {
            if (await _authService.Invalidate(request)) return NoContent();

            return Unauthorized();
        }
    }
}