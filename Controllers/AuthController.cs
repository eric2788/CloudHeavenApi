using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudHeavenApi.Contexts;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudHeavenApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly HeavenContext _context;
        private readonly AuthService _authService;

        public AuthController(HeavenContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }


        [HttpPost("authenticate")]
        public async Task<ActionResult<TokenProfile>> Authenticate([FromBody] AuthenticateRequest request)
        {
            TokenProfile tokenProfile;
            try
            {
                tokenProfile = await _authService.Authenticate(request);
            }
            catch (AuthException e)
            {
                return Unauthorized(e.ErrorResponse);
            }

            var ac = await _context.WebAccounts.AsNoTracking().FirstOrDefaultAsync(ac => ac.Uuid == tokenProfile.UUID);
            if (ac == null)
            {
                return Unauthorized(new { error = "尚未在伺服器登入。"});
            }

            if (ac.UserName == tokenProfile.UserName) return Ok(ac);


            ac.UserName = tokenProfile.UserName;
            _context.Entry(ac).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                //throw;
                return BadRequest(e);
            }

            return Ok(ac);

        }

        [HttpPost("signout")]
        public async Task<ActionResult> SignOut([FromBody] AuthorizeRequest request)
        {
            if (await _authService.Invalidate(request))
            {
                return Ok();
            }

            return Unauthorized();
        }


    }
}
