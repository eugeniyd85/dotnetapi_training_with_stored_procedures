using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth = "INSERT INTO TutorialAppSchema.Auth (Email, PasswordHash, PasswordSalt) VALUES ('" + userForRegistration.Email + "', @PasswordHash, @PasswordSalt)";

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;

                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {

                        string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users (FirstName, LastName, Email, Gender, Active)
                            VALUES ('" + userForRegistration.FirstName + @"', '" + userForRegistration.LastName + @"', '" + userForRegistration.Email + @"', '" + userForRegistration.Gender + @"', 1)";

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

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"SELECT 
                [PasswordHash],
                [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = '" + 
                userForLogin.Email + "'";

            UserForLoginConfirmationDto userForConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

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
                {"token", CreateToken(userId)}
            });
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8);
        }

        private string CreateToken(int userId)
        {
            // Create claims for the token, including the user ID as a claim
            Claim[] claims = new Claim[]
            {
                new Claim("userId", userId.ToString())
            };

            // Get the token key from configuration and create signing credentials
            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            // Ensure the token key is not null before creating the SymmetricSecurityKey
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyString!=null ? tokenKeyString : ""));

            // Create signing credentials using the token key and specify the hashing algorithm
            SigningCredentials credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha256Signature);

            // Create a token descriptor that includes the claims, expiration time, and signing credentials
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            // Create a token handler to generate the JWT token based on the token descriptor
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            // Generate the token using the token handler and the token descriptor
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            // Return the generated token as a string
            return tokenHandler.WriteToken(token);
        }
    }
}