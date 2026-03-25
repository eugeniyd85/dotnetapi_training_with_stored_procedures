
using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

// obsolete controller that was use for training simple CRUD operations with Dapper without using stored procedures. UserCompleteController is user now
[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
    // DataContextEF _entityFramework; // was used before implementing repository pattern, now we will use IUserRepository to call the methods defined in UserRepository.cs for performing database operations related to users instead of directly using the DataContextEF class in the controller
    IUserRepository _userRepository;
    IMapper _mapper;

    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        // _entityFramework = new DataContextEF(config); //injected configuration is used to create an instance of the DataContextEF class which will be used to interact with the database using Entity Framework
        _userRepository = userRepository; //injected IUserRepository is assigned to the _userRepository field which will be used to call the methods defined in the IUserRepository interface and implemented in the UserRepository class for performing database operations related to users
        _mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDto, User>();
            cfg.CreateMap<UserSalary, UserSalary>();
            cfg.CreateMap<UserJobInfo, UserJobInfo>();
        }));
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    [HttpGet("GetSingleUser/{userId}")]
    public User GetSingleUser(int userId)
    {
        // User? user = _entityFramework.Users.Where(u => u.UserId == userId).FirstOrDefault();

        // if (user != null)
        // {
        //     return user;
        // }

        // throw new Exception("User not found");
        return _userRepository.GetSingleUser(userId);
    }

    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        User? userDb = _userRepository.GetSingleUser(user.UserId);

        if (userDb != null)
        {
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;
            userDb.Active = user.Active;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Edit User with ID " + user.UserId);
        }

        throw new Exception("User not found");
    }

    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        //with automapper
        User userDb = _mapper.Map<User>(user);
        

        //without automapper
        // User userDb = new User();

        // userDb.FirstName = user.FirstName;
        // userDb.LastName = user.LastName;
        // userDb.Email = user.Email;
        // userDb.Gender = user.Gender;
        // userDb.Active = user.Active;

        _userRepository.AddEntity<User>(userDb);

        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userDb = _userRepository.GetSingleUser(userId);

        if (userDb != null)
        {
            _userRepository.RemoveEntity<User>(userDb);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User with ID " + userId);
        }

        throw new Exception("User not found");
    }

    [HttpGet("UserSalary/{userId}")]
    public UserSalary GetUserSalary(int userId)
    {
        // UserSalary? userSalary = _entityFramework.UserSalary.Where(us => us.UserId == userId).FirstOrDefault();

        // if (userSalary != null)
        // {
        //     return userSalary;
        // }

        // throw new Exception("User salary not found");
        return _userRepository.GetUserSalary(userId);
    }

    // alternative way to get user salary
    // [HttpGet("UserSalary/{userId}")]
    // public IEnumerable<UserSalary> GetUserSalaryAlt(int userId)
    // {
    //     return _entityFramework.UserSalary.Where(us => us.UserId == userId).ToList();
    // }



    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary(UserSalary userSalary)
    {
        UserSalary? userSalaryDb = _userRepository.GetUserSalary(userSalary.UserId);

        if (userSalaryDb != null)
        {
            userSalaryDb.Salary = userSalary.Salary;

            // alternative way to use mapping for updating user salary fields
            // _mapper.Map(userSalary, userSalaryDb);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Edit User salary with ID " + userSalary.UserId);
        }

        throw new Exception("User salary not found");
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userSalaryForInsert)
    {
        _userRepository.AddEntity<UserSalary>(userSalaryForInsert);

        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User Salary");
    }

    [HttpDelete("DeleteUserSalary/{userId}")]
    public IActionResult DeleteUserSalary(int userId)
    {
        UserSalary? userSalaryDb = _userRepository.GetUserSalary(userId);

        if (userSalaryDb != null)
        {
            _userRepository.RemoveEntity<UserSalary>(userSalaryDb);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User salary with ID " + userId);
        }

        throw new Exception("User salary not found");
    }

    [HttpGet("UserJobInfo/{userId}")]
    public UserJobInfo GetUserJobInfo(int userId)
    {
        // UserJobInfo? userJobInfo = _entityFramework.UserJobInfo.Where(uj => uj.UserId == userId).FirstOrDefault();

        // if (userJobInfo != null)
        // {
        //     return userJobInfo;
        // }

        // throw new Exception("User job info not found");
        return _userRepository.GetUserJobInfo(userId);
    }

    [HttpPut("EditUserJobInfo")]
    public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
    {
        UserJobInfo? userJobInfoDb = _userRepository.GetUserJobInfo(userJobInfo.UserId);

        if (userJobInfoDb != null)
        {
            userJobInfoDb.JobTitle = userJobInfo.JobTitle;
            userJobInfoDb.Department = userJobInfo.Department;

            // alternative way to use mapping for updating user job info fields
            // _mapper.Map(userJobInfo, userJobInfoDb);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Edit User job info with ID " + userJobInfo.UserId);
        }

        throw new Exception("User job info not found");
    }

    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
    {
        _userRepository.AddEntity<UserJobInfo>(userJobInfo);

        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User Job Info");
    }

    [HttpDelete("DeleteUserJobInfo/{userId}")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfoDb = _userRepository.GetUserJobInfo(userId);

        if (userJobInfoDb != null)
        {
            _userRepository.RemoveEntity<UserJobInfo>(userJobInfoDb);

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User job info with ID " + userId);
        }

        throw new Exception("User job info not found");
    }
}