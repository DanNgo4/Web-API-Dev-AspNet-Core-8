using AuthenticationDemo.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController(UserManager<AppUser> userManager, IConfiguration configuration)
           : ControllerBase
{
    private readonly UserManager<AppUser> _userManager   = userManager;
    private readonly IConfiguration       _configuration = configuration;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AddOrUpdateAppUserModel model)
    {
        if (ModelState.IsValid)
        {
            var existedUser = await _userManager.FindByNameAsync(model.UserName);
            if (existedUser is not null)
            {
                ModelState.AddModelError("", "User name is already taken");
                return BadRequest(ModelState);
            }
        }

        var user = new AppUser()
        {
            UserName      = model.UserName,
            Email         = model.Email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            var token = _configuration.GenerateToken(model.UserName);
            return Ok(new { token });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return BadRequest(ModelState);
    }
}
