﻿using System.Threading.Tasks;
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
    public class ResourceController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly HeavenContext _context;

        public ResourceController(HeavenContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("badges")]
        public async Task<ActionResult> GetBadges([FromBody] AuthorizeRequest request)
        {
            var tokenProfile = await _authService.Validate(request);
            var self = await _context.WebAccounts.FindAsync(tokenProfile.UUID);
            if (self == null) return NotFound(new {Error = $"Account {tokenProfile.UUID} not found"});

            if (!self.Admin) return Unauthorized(new {Error = "You are not admin"});

            return Ok(await _context.Badges.ToListAsync());
        }

        [HttpPost("badge")]
        public async Task<ActionResult> CreateBadge([FromBody] BadgeEditor editor)
        {
            var tokenProfile = await _authService.Validate(editor.Request);
            var self = await _context.WebAccounts.FindAsync(tokenProfile.UUID);
            if (self == null) return NotFound(new {Error = $"Account {tokenProfile.UUID} not found"});

            if (!self.Admin) return Unauthorized(new {Error = "You are not admin"});

            await _context.Badges.AddAsync(editor.Badge);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("badge/{id}")]
        public async Task<ActionResult> GetBadge(int id, [FromBody] AuthorizeRequest request)
        {
            var tokenProfile = await _authService.Validate(request);
            var self = await _context.WebAccounts.FindAsync(tokenProfile.UUID);
            if (self == null) return NotFound(new {Error = $"Account {tokenProfile.UUID} not found"});

            if (!self.Admin) return Unauthorized(new {Error = "You are not admin"});

            return Ok(await _context.Badges.FindAsync(id));
        }

        [HttpPut("badge/{id}")]
        public async Task<ActionResult> UpdateBadge(int id, [FromBody] BadgeEditor editor)
        {
            var tokenProfile = await _authService.Validate(editor.Request);
            var self = await _context.WebAccounts.FindAsync(tokenProfile.UUID);
            if (self == null) return NotFound(new {Error = $"Account {tokenProfile.UUID} not found"});

            if (!self.Admin) return Unauthorized(new {Error = "You are not admin"});

            if (editor.Badge.BadgeId != id)
                return BadRequest(new {Error = "Edit id does not match the badge id from body"});

            try
            {
                _context.Entry(editor.Badge).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _context.Badges.AsNoTracking().AnyAsync(s => s.BadgeId == id))
                    return NotFound(new {Error = $"Badge {id} NotFound"});

                throw;
            }

            return NoContent();
        }

        [HttpDelete("badge/{id}")]
        public async Task<ActionResult> DeleteBadge(int id, [FromBody] AuthorizeRequest request)
        {
            var tokenProfile = await _authService.Validate(request);
            var self = await _context.WebAccounts.FindAsync(tokenProfile.UUID);
            if (self == null) return NotFound(new {Error = $"Account {tokenProfile.UUID} not found"});

            if (!self.Admin) return Unauthorized(new {Error = "You are not admin"});

            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) return NotFound(new {Error = $"Badge {id} not found"});

            _context.Badges.Remove(badge);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class BadgeEditor
    {
        public AuthorizeRequest Request { get; set; }
        public Badge Badge { get; set; }
    }
}