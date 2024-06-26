using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

namespace SpotifyCaster.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TestController : ControllerBase
{
    public TestController()
    {
        
    }
    
    [HttpGet]
    public async Task<IActionResult> Test()
    {
        
        
        return Ok();
    }
}