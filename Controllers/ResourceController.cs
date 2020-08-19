using System.Threading.Tasks;
using CloudHeavenApi.Contexts;
using CloudHeavenApi.Models;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloudHeavenApi.Controllers
{
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

        [HttpPost]
        public async Task<ActionResult> PingResource([FromBody] AuthorizeRequest request)
        {
            var profile = await _authService.Validate(request);

            return Ok(profile);
        }
    }
}