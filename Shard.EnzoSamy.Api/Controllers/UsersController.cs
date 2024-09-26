using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Contracts;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;

[Route("[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPut]
    [Route("/users/{userId}")]
    public ActionResult<UserSpecification> PutPlayer(string userId, [FromBody] UserRequest newUser)
    {
        if (userId != newUser.Id)
        {
            return BadRequest("The userId in the URL does not match the Id in the body.");
        }

        if (!ValidationUtils.IsValidUserId(userId))
        {
            return BadRequest("The body does not contain a valid identifier.");
        }

        try
        {
            var user = _userService.CreateUser(newUser);
            return user;
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while updating the user.");
        }
    }

    [HttpGet]
    [Route("/users/{userId}")]
    public ActionResult<UserSpecification> GetOnePlayer(string userId)
    {
        try
        {
            var user = _userService.FindUser(userId);

            if (user == null) return NotFound($"User with ID {userId} not found.");

            return user;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "An error occurred while getting the user.");
        }
    }
}