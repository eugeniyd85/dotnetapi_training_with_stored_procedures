using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";
                
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    UserForLoginDto userForSetPassword = new UserForLoginDto
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password
                    };
                    if (_authHelper.SetPassword(userForSetPassword))
                    {

                        string sqlAddUser = @"EXEC TutorialAppSchema.spUser_Upsert
                                @FirstName = '" + userForRegistration.FirstName + @"',
                                @LastName = '" + userForRegistration.LastName + @"',
                                @Email = '" + userForRegistration.Email + @"',
                                @Gender = '" + userForRegistration.Gender + @"',
                                @Active = 1" + @",
                                @JobTitle = '" + userForRegistration.JobTitle + @"',
                                @Department = '" + userForRegistration.Department + @"',
                                @Salary = " + userForRegistration.Salary.ToString(CultureInfo.InvariantCulture);

                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user");
                    }
                    throw new Exception("Failed to register user");
                }
                throw new Exception("User with this email already exists");

            }
            throw new Exception("Passwords do not match");
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForResetPassword)
        {
            if (_authHelper.SetPassword(userForResetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to reset password");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"EXEC TutorialAppSchema.spLoginConfirmation_Get
                @Email = @EmailParam";

            DynamicParameters sqlParameters = new DynamicParameters();

            // SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
            // emailParameter.Value = userForLogin.Email;
            // sqlParameters.Add(emailParameter);
            sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

            UserForLoginConfirmationDto userForConfirmation = _dapper
                .LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for (int i = 0; i < passwordHash.Length; i++)
            {
                if (passwordHash[i] != userForConfirmation.PasswordHash[i])
                {
                    // throw new Exception("Invalid password");
                    return StatusCode(401, "Invalid password");
                }
            }
            
            string getUserIdSql = "SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" + userForLogin.Email + "'";

            int userId = _dapper.LoadDataSingle<int>(getUserIdSql);

            return Ok(new Dictionary<string, string>
            {
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            // Get the user ID from the claims in the current token
            string userId = User.FindFirst("userId")?.Value + "";

            string userIdSql = "SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = '" + userId + "'";

            int userIdFromDb = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string>
            {
                {"token", _authHelper.CreateToken(userIdFromDb)}
            });
        }
    }
}