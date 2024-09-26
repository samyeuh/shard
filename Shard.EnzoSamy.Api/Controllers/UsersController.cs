using Microsoft.AspNetCore.Mvc;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;

[Route("[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly List<UserSpecification> _users;
    private readonly UserService _userService;

    public UsersController(List<UserSpecification> users, UserService userService)
    {
        _users = users;
        _userService = userService;
    }

    [HttpPut]
    [Route("/users/{userId}")]
    public ActionResult<UserSpecification> PutPlayer(string userId, [FromBody] UserSpecification updatedUser)
    {
        if (userId != updatedUser.Id)
        {
            return BadRequest("The userId in the URL does not match the Id in the body.");
        }

        if (!ValidationUtils.IsValidUserId(userId))
        {
            return BadRequest("The body does not contain a valid identifier.");
        }

        try
        {
            var index = _userService.FindUserIndex(userId);

            if (index == -1)
            {
                _users.Add(updatedUser);
            }
            else
            {
                _users[index] = updatedUser;
            }

            return updatedUser;
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
            var index = _userService.FindUserIndex(userId);

            if (index != -1)
            {
                return _users[index];
            }

            return NotFound($"User with ID {userId} not found. Actual users: {_users}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "An error occurred while getting the user.");
        }
    }
}