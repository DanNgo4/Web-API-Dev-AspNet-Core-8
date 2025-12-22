using AuthenticationDemo.Models.Authentication;
using AuthenticationDemo.Models.Role;
using AuthenticationDemo.Models.User;
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

        // Try to save the user
        var userResult = await _userManager.CreateAsync(user, model.Password);

        // Add the user to the "User" role
        var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.User);

        if (userResult.Succeeded && roleResult.Succeeded)
        {
            var token = _configuration.GenerateToken(_userManager, user);
            return Ok(new { token });
        }

        foreach (var error in userResult.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        foreach (var error in roleResult.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // Get the secret in the configuration

        // Check if the model is valid
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user is not null)
            {
                var pwdValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (pwdValid)
                {
                    var token = _configuration.GenerateToken(_userManager, user);
                    return Ok(new { token });
                }
            }

            // If the user is not found, display an error message
            ModelState.AddModelError("", "Invalid username or password");
        }

        return BadRequest(ModelState);
    }
}
