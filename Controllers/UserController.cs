using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class UserController : ControllerBase
    {
        private readonly HeavenContext _context;
        private readonly AuthService _authService;

        public UserController(HeavenContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet("User")]
        public async Task<ActionResult> GetUsers([FromBody] AuthorizeRequest request, [FromQuery] int page)
        {
            TokenProfile tokenProfile;
            try
            {
                tokenProfile = await _authService.Refresh(request);
            }
            catch (AuthException e)
            {
                return Unauthorized(e.ErrorResponse);
            }

            var list = await _context.WebAccounts.Skip(30 * Math.Max(0, page - 1)).Take(30)
                .Include(s => s.PersonBadgeses.Select(b => b.Badge))
                .Select(s => new User
                {
                    Admin = s.Admin,
                    Badges = s.PersonBadgeses.Where(b => b.Uuid == s.Uuid).Select(b => b.Badge).ToArray(),
                    NickName = s.NickName,
                    UserName = s.UserName,
                    Uuid = s.Uuid
                }).ToListAsync();

            return Ok(new {list, tokenProfile});
        }
    }
}
