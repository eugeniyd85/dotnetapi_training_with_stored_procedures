using System.Globalization;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
    
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
         return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers")]
    // public IActionResult Test()
    public IEnumerable<User> GetUsers()
    {
        string sql = "SELECT * FROM TutorialAppSchema.Users";
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
    }

    [HttpGet("GetSingleUser/{userId}")]
    // public IActionResult Test()
    public User GetSingleUser(int userId)
    {
        string sql = "SELECT * FROM TutorialAppSchema.Users WHERE UserId = " + userId.ToString();
        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string sql = @"UPDATE TutorialAppSchema.Users
                       SET FirstName = '" + user.FirstName + @"',
                           LastName = '" + user.LastName + @"',
                           Email = '" + user.Email + @"',
                           Gender = '" + user.Gender + @"',
                           Active = '" + user.Active + @"'
                       WHERE UserId = " + user.UserId;
        
        // Alternative way to write the SQL string. !!!!Investigate the difference. 
        // In 2nd case less spaces are used? Clarify regarding vulnerability of the 2nd approach.
        // string sql = @"
        // UPDATE TutorialAppSchema.Users
        //     SET [FirstName] = '" + user.FirstName + 
        //         "', [LastName] = '" + user.LastName +
        //         "', [Email] = '" + user.Email + 
        //         "', [Gender] = '" + user.Gender + 
        //         "', [Active] = '" + user.Active + 
        //     "' WHERE UserId = " + user.UserId;


        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to update user with ID " + user.UserId);
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string sql = @"INSERT INTO TutorialAppSchema.Users (FirstName, LastName, Email, Gender, Active)
                       VALUES ('" + user.FirstName + @"', '" + user.LastName + @"', '" + user.Email + @"', '" + user.Gender + @"', '" + user.Active + @"')";
        
        Console.WriteLine(sql);
        
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        
        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.Users WHERE UserId = " + userId.ToString();
        
        Console.WriteLine(sql);
        
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        
        throw new Exception("Failed to Delete User with ID " + userId);
    }

    [HttpGet("UserSalary/{userId}")]
    public UserSalary GetUserSalary(int userId)
    {
        return _dapper.LoadDataSingle<UserSalary>("SELECT * FROM TutorialAppSchema.UserSalary WHERE UserId = " + userId);
    }

    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary(UserSalary userSalaryForUpdate)
    {
        string sql = @"UPDATE TutorialAppSchema.UserSalary
                       SET Salary = " + userSalaryForUpdate.Salary.ToString(CultureInfo.InvariantCulture) +
                       " WHERE UserId = " + userSalaryForUpdate.UserId;
        
        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userSalaryForUpdate);
        }
        throw new Exception("Failed to update User Salary");
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userSalaryToAdd)
    {
        // using 'userSalaryToAdd.Salary.ToString(CultureInfo.InvariantCulture)' to avoid issues with decimal separator 
        // in different cultures (e.g., comma vs dot)
        // but this is a very basic way to handle it and can still be vulnerable to SQL injection.
        // the correct way to handle this is to use parameterized queries or an ORM that supports them, 
        // which would also help prevent SQL injection attacks.
        // CORRECT way to write the SQL string with parameters (using Dapper's parameterization):
        // const string sql = """
        // INSERT INTO TutorialAppSchema.UserSalary (UserId, Salary)
        // VALUES (@UserId, @Salary)
        // """;
        // with next ExecuteSql wrapper:
        // public bool ExecuteSql(string sql, object? parameters = null)
        // {
        //     return _connection.Execute(sql, parameters) > 0;
        // }


        string sql = @"INSERT INTO TutorialAppSchema.UserSalary (UserId, Salary)
                       VALUES (" + userSalaryToAdd.UserId + ", " + userSalaryToAdd.Salary.ToString(CultureInfo.InvariantCulture) + ")";

        Console.WriteLine(sql);
        Console.WriteLine(userSalaryToAdd.Salary.GetType());
        
        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userSalaryToAdd);
        }
        
        throw new Exception("Failed to Add User Salary");
    }

    [HttpDelete("DeleteUserSalary/{userId}")]
    public IActionResult DeleteUserSalary(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.UserSalary WHERE UserId = " + userId;
        
        Console.WriteLine(sql);
        
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        
        throw new Exception("Failed to Delete User Salary");
    }
}