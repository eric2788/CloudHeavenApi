using System;
using System.Linq;
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
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly HeavenContext _context;

        public UserController(HeavenContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("users")]
        public async Task<ActionResult> GetUsers([FromBody] AuthorizeRequest request, [FromQuery] int page = 1)
        {
            await _authService.Validate(request);
            var list = await _context.WebAccounts.Skip(30 * Math.Max(0, page - 1)).Take(30).Select(ac => new User(ac))
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<ActionResult> GetSelf([FromBody] AuthorizeRequest request)
        {
            var tokenProfile = await _authService.Validate(request);
            var accounts = await _context.WebAccounts.Where(s => s.Uuid == tokenProfile.UUID).ToArrayAsync();
            var self = accounts
                .GroupJoin(
                    _context.PersonBadges.Include(b => b.Badge),
                    account => account.Uuid, bd => bd.Uuid,
                    (account, list) => new {account, Badges = list.Select(b => b.Badge)})
                .Join(_context.Cmis, ac => ac.account.Uuid, cmi => cmi.playerUUID,
                    (ac, cmi) => new {ac.account, ac.Badges, cmi}).FirstOrDefault();
            if (self == null)
                return NotFound(new ErrorResponse
                {
                    Error = "404 Not Found",
                    ErrorMessage = $"Cannot Find Account {tokenProfile.UUID}"
                });
            return Ok(self);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateSelf([FromBody] AccountEditor editor)
        {
            var tokenProfile = await _authService.Validate(editor.Request);
            var toUpdate = await _context.WebAccounts.AsNoTracking()
                .FirstOrDefaultAsync(ac => ac.Uuid == tokenProfile.UUID);
            if (toUpdate == null)
                return NotFound(new ErrorResponse
                {
                    Error = "Account NotFound",
                    ErrorMessage = $"Cannot Found The {tokenProfile.UUID} Account"
                });

            if (!editor.Editor.CanDeserialize<NormalEditor>())
                return BadRequest(new {error = "invalid editor pattern"});

            var edit = editor.Editor.JsonDeserialize<NormalEditor>();

            toUpdate.NickName = edit.NickName;
            toUpdate.Status = edit.Status;

            _context.Entry(toUpdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> GetUser(string id, [FromBody] AuthorizeRequest request)
        {
            await _authService.Validate(request);
            var uuid = new Guid(id);
            var list = await _context.WebAccounts.Where(s => s.Uuid == uuid).ToListAsync();
            var user = list.GroupJoin(_context.PersonBadges.Include(b => b.Badge),
                    account => account.Uuid, bd => bd.Uuid,
                    (account, list) => new {account, Badges = list.Select(b => b.Badge)})
                .Join(_context.Cmis, ac => ac.account.Uuid, cmi => cmi.playerUUID,
                    (ac, cmi) => new {ac.account, ac.Badges, cmi}).FirstOrDefault();
            if (user == null)
                return NotFound(new ErrorResponse
                {
                    Error = "404 Not Found",
                    ErrorMessage = $"Cannot Find Account {uuid}"
                });
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] AccountEditor editor)
        {
            var profile = await _authService.Validate(editor.Request);
            var uuid = new Guid(id);
            var user = await _context.WebAccounts.AsNoTracking().FirstOrDefaultAsync(s => s.Uuid == uuid);
            if (user == null)
                return NotFound(new ErrorResponse
                {
                    Error = "Account NotFound",
                    ErrorMessage = $"Cannot Found The {uuid} Account"
                });

            var self = await _context.WebAccounts.AsNoTracking().FirstOrDefaultAsync(s => s.Uuid == profile.UUID);
            if (self is null)
                return NotFound(new ErrorResponse
                {
                    Error = "Account NotFound",
                    ErrorMessage = $"Cannot Found The {profile.UUID} Account"
                });

            if (!self.Admin)
                throw new AuthException(new ErrorResponse
                {
                    Error = "No Permission",
                    ErrorMessage = $"Account {profile.UUID} is not admin"
                });

            if (!editor.Editor.CanDeserialize<NormalEditor>())
                return BadRequest(new {error = "invalid editor pattern"});

            var edit = editor.Editor.JsonDeserialize<NormalEditor>();
            user.NickName = edit.NickName;
            user.Status = edit.Status;

            if (editor.Editor.CanDeserialize<AdminEditor>())
            {
                var adminEditor = editor.Editor.JsonDeserialize<AdminEditor>();
                user.Admin = adminEditor.Admin;

                var userBadges = await _context.PersonBadges.AsNoTracking().Where(s => s.Uuid == uuid).ToListAsync();
                _context.PersonBadges.RemoveRange(userBadges);
                await _context.SaveChangesAsync();
                var toAdd = adminEditor.Badges.Select(bid => new PersonBadges
                {
                    Uuid = uuid,
                    BadgeId = bid
                }).ToList();
                _context.PersonBadges.AddRange(toAdd);
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class AccountEditor
    {
        public AuthorizeRequest Request { get; set; }
        public object Editor { get; set; }
    }

    public class NormalEditor
    {
        public string NickName { get; set; }
        public string Status { get; set; }
    }

    public class AdminEditor : NormalEditor
    {
        public int[] Badges { get; set; }
        public bool Admin { get; set; }
    }
}