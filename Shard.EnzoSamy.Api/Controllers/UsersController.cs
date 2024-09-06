using Microsoft.AspNetCore.Mvc;

namespace Shard.EnzoSamy.Api.Controllers;


[Route("[controller]")]
[ApiController]

public class UsersController : ControllerBase
{
    private List<UserSpecification> _users;
    
    public UsersController(List<UserSpecification> users)
    {
        _users = users;
    }
    
    [HttpPut]
    [Route("/users/{userId}")]
    public ActionResult<UserSpecification> PutPlayer(string userId, [FromBody] UserSpecification updatedUser)
    {
        if (userId != updatedUser.Id)
        {
            return BadRequest("The userId in the URL does not match the Id in the body.");
        }

        try
        {
            int index = _users.FindIndex(user => user.Id == userId);

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
        catch (Exception e)
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
            int index = _users.FindIndex(user => user.Id == userId);
            
            if (index != -1)
            {
                return  _users[index]; 
            }
            else
            {
                return NotFound($"User with ID {userId} not found. Actual users : {_users}"); 
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "An error occurred while getting the user."); 
        }
    }

    
}