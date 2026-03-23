using System.Globalization;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
    DataContextDapper _dapper;
    
    public UserCompleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
         return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    // still can't set isActive as optional parameter in Swagger UI. Find out how to do it.
    [HttpGet("GetUsers/{userId}/{isActive?}")]
    // public IActionResult Test()
    public IEnumerable<UserComplete> GetUsers(int userId, bool? isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        string parameters = "";
        
        if (userId != 0)
        {
            parameters += ", @UserId = " + userId.ToString();
        }

        if (isActive.HasValue)
        {
            parameters += ", @Active = " + isActive.Value.ToString();
        }

        sql += parameters.Substring(1); // Remove the leading comma and space

        Console.WriteLine(sql);
         
        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
        return users;
    }

    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Upsert
                       @FirstName = '" + user.FirstName + @"',
                       @LastName = '" + user.LastName + @"',
                       @Email = '" + user.Email + @"',
                       @Gender = '" + user.Gender + @"',
                       @JobTitle = '" + user.JobTitle + @"',
                       @Department = '" + user.Department + @"',
                       @Salary = " + user.Salary.ToString(CultureInfo.InvariantCulture) + @",
                       @Active = " + user.Active + @",
                       @UserId = " + user.UserId;

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to update user with ID " + user.UserId);
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Delete @UserId = " + userId.ToString();
        
        Console.WriteLine(sql);
        
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        
        throw new Exception("Failed to Delete User with ID " + userId);
    }
}