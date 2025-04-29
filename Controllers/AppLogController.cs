using ChatApp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppLogController(IAppLogsRepository appLogsRepository) : ControllerBase
    {
        private readonly IAppLogsRepository _repo = appLogsRepository;

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            try
            {
                var data = await _repo.GetLogs();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
