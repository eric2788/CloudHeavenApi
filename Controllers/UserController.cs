using System;
using System.Linq;
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
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly HeavenContext _context;

        public UserController(HeavenContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost]
        public async Task<ActionResult> GetUsers([FromBody] AuthorizeRequest request, [FromQuery] int page = 1)
        {
            var tokenProfile = await _authService.Validate(request);
            var accounts = await _context.WebAccounts.Skip(30 * Math.Max(0, page - 1)).Take(30).ToListAsync();
            var list = accounts.GroupJoin(
                _context.PersonBadges, ac => ac.Uuid, pb => pb.Uuid, (ac, pb) => new User(ac)
                {
                    Badges = pb.DefaultIfEmpty().Select(s => s.Badge).ToArray()
                });
            return Ok(new {list, tokenProfile});
        }
    }
}