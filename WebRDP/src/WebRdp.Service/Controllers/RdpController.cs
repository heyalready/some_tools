using Microsoft.AspNetCore.Mvc;
using WebRdp.Client;
using WebRdp.Service.Models;
using WebRdp.Service.Services;

namespace WebRdp.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RdpController : ControllerBase
    {
        private readonly IRdpSessionManager _sessionManager;
        private readonly ILogger<RdpController> _logger;

        public RdpController(IRdpSessionManager sessionManager, ILogger<RdpController> logger)
        {
            _sessionManager = sessionManager;
            _logger = logger;
        }

        [HttpPost("connect")]
        public async Task<ActionResult<RdpSession>> Connect([FromBody] RdpConnectionConfig config)
        {
            _logger.LogInformation("Connect API called");
            try
            {
                var session = await _sessionManager.ConnectAsync(config);
                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connect failed");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("disconnect/{sessionId}")]
        public async Task<IActionResult> Disconnect(string sessionId)
        {
            _logger.LogInformation($"Disconnect API called for session {sessionId}");
            try
            {
                await _sessionManager.DisconnectAsync(sessionId);
                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Disconnect failed");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("status/{sessionId}")]
        public async Task<ActionResult<RdpSessionStatus>> GetStatus(string sessionId)
        {
            var status = await _sessionManager.GetStatusAsync(sessionId);
            return Ok(status);
        }

        [HttpPost("input/{sessionId}")]
        public async Task<IActionResult> SendInput(string sessionId, [FromBody] InputEvent input)
        {
            await Task.Yield();
            return Ok(new { Success = true });
        }

        [HttpGet("stream")]
        public async Task GetStream()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Task.CompletedTask;
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }
}
