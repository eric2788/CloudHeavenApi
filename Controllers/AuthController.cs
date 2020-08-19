using System.Threading.Tasks;
using CloudHeavenApi.Contexts;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudHeavenApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        private readonly HeavenContext _context;

        public AuthController(HeavenContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }


        [HttpPost("authenticate")]
        public async Task<ActionResult<TokenProfile>> Authenticate([FromBody] AuthenticateRequest request)
        {
            var tokenProfile = await _authService.Authenticate(request);

            /*
            var ac = await _context.WebAccounts.AsNoTracking().FirstOrDefaultAsync(account => account.Uuid == tokenProfile.UUID);
            if (ac == null)
            {
                await _authService.Invalidate(new AuthorizeRequest
                {
                    AccessToken = tokenProfile.AccessToken,
                    ClientToken = tokenProfile.ClientToken
                });
                throw new AuthException(new ErrorResponse
                {
                    Error = "未知的賬戶",
                    ErrorMessage = "你從未到伺服器登入過。"
                });
            }

            if (ac.UserName == tokenProfile.UserName) return Ok(ac);


            ac.UserName = tokenProfile.UserName;
            _context.Entry(ac).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            */

            var wa = new WebAccount
            {
                Admin = true,
                NickName = "陳大明",
                UserName = tokenProfile.UserName,
                Uuid = tokenProfile.UUID,
                Status = "沒有狀態"
            };
            return Ok(new
            {
                user = new User(wa),
                token = new
                {
                    accessToken = tokenProfile.AccessToken,
                    clientToken = tokenProfile.ClientToken
                }
            });
        }

        [HttpPost("signout")]
        public async Task<ActionResult> SignOut([FromBody] AuthorizeRequest request)
        {
            if (await _authService.Invalidate(request)) return Ok();

            return Unauthorized();
        }
    }
}