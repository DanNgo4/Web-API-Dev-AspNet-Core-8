using Microsoft.AspNetCore.Mvc;
using MyFirstApi.Models;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Post>> GetPosts()
    {
        return new List<Post>
        {
            new() { Id = 1, UserId = 1, Title = "Post1", Body = "The first post." },
            new() { Id = 2, UserId = 1, Title = "Post2", Body = "The second post." },
            new() { Id = 3, UserId = 2, Title = "Post3", Body = "The third post." }
        };
    }
}
