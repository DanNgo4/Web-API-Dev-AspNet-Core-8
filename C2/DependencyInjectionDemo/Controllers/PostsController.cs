using Microsoft.AspNetCore.Mvc;
using DependencyInjectionDemo.Models;
using DependencyInjectionDemo.Interfaces;

namespace DependencyInjectionDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postsService;

    // the service container will inject the correct implementation (service) of IPostService interface into the controller
    public PostsController(IPostService postService)
    {
        _postsService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Post>>> GetPosts()
    {
        var posts = await _postsService.GetAllPosts();
        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(int id)
    {
        var post = await _postsService.GetPost(id);
        if (post == null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost(Post post)
    {
        await _postsService.CreatePost(post);

        // Retuns a response message with the specified action name, route values and post.
        // Call the GetPost() action/method to return the newly created post
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePost(int id, Post post)
    {
        if (id != post.Id)
        {
            return BadRequest();
        }

        var updatedPost = await _postsService.UpdatePost(id, post);
        if (updatedPost == null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePost(int id)
    {
        var post = await _postsService.GetPost(id);
        if (post == null) return NotFound();

        await _postsService.DeletePost(id);
        return Ok();
    }
}
