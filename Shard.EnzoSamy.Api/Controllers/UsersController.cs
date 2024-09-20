using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
using Shard.EnzoSamy.Api.Services;
using Shard.EnzoSamy.Api.Specifications;
using Shard.EnzoSamy.Api.Utilities;

namespace Shard.EnzoSamy.Api.Controllers;


[Route("[controller]")]
[ApiController]

public class UsersController(List<UserSpecification> users, UserService userService) : ControllerBase
{
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
            var index = userService.FindUserIndex(userId);

            if (index == -1)
            {
                users.Add(updatedUser);
            }
            else
            {
                users[index] = updatedUser;
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
            var index = userService.FindUserIndex(userId);
            
            if (index != -1)
            {
                return  users[index]; 
            }

            return NotFound($"User with ID {userId} not found. Actual users : {users}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "An error occurred while getting the user."); 
        }
    }
   

    
}