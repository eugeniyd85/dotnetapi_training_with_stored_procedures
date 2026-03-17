using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;

        public AuthHelper(IConfiguration config)
        {
            _config = config;
        }

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8);
        }

        public string CreateToken(int userId)
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