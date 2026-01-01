using AuthenticationDemo.Models.Authentication;
using AuthenticationDemo.Models.Role;
using AuthenticationDemo.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController(UserManager<AppUser> userManager, 
                               IConfiguration configuration, 
                               SignInManager<AppUser> signInManager)
           : ControllerBase
{
    private readonly IConfiguration         _configuration = configuration;
    private readonly UserManager<AppUser>   _userManager   = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AddOrUpdateAppUserModel model)
    {
        // Check if the email is unique
        var existingEmail = await _userManager.FindByEmailAsync(model.Email);
        if (existingEmail is not null)
        {
            ModelState.AddModelError("Email", "Email already exists");
            return BadRequest(ModelState);
        }

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
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);
                if (result.Succeeded)
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

    [HttpPost("loginNewZealand")]
    public async Task<IActionResult> LoginNewZealand([FromBody] LoginModel model)
    {
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

            ModelState.AddModelError("", "Invalid username or password");
        }

        return BadRequest(ModelState);
    }
}
