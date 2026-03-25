using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    // sets whatever before "Cotroller" in the class name as a base of the route 
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = "EXEC TutorialAppSchema.spPosts_Get";
            string stringParameters = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            if (postId != 0)
            {
                stringParameters += ", @PostId = @PostIdParameter";
                sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);
            }

            if (userId != 0)
            {
                stringParameters += ", @UserId = @UserIdParameter";
                sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            }

            if (searchParam.ToLower() != "none")
            {
                stringParameters += ", @SearchValue = @SearchValueParameter";
                sqlParameters.Add("@SearchValueParameter", searchParam, DbType.String);
            }

            // compare with '!string.IsNullOrEmpty(parameters)' in UserCompleteController.cs, what is better to use or the same? => both are fine, but the one with 'parameters.Length > 0' is more efficient because it does not need to call a method, it just checks the length of the string, while the one with '!string.IsNullOrEmpty(parameters)' needs to call the IsNullOrEmpty method which checks if the string is null or empty, and then returns a boolean value, so it is less efficient than just checking the length of the string
            if (stringParameters.Length > 0)
            {
                sql += stringParameters.Substring(1); // remove the first comma
            }

            return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
        }

        // should only return posts of the user that is currently logged in, so we can get the user id from the token and use it in the sql query
        // how the user id from the token is getting? we can get it from the User property of the controller, which is of type ClaimsPrincipal, and we can use the FindFirst method to find the claim with the type "UserId" and get its value
        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get
                @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);

            return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            // find out why here 'this.User.FindFirst("userId")?.Value' UserId caused and error but "userId" works fine
            string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
                @UserId = @UserIdParameter,
                @PostTitle = @PostTitleParameter,
                @PostContent = @PostContentParameter";
            
            DynamicParameters sqlParameters = new DynamicParameters();
            
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParameters.Add("@PostTitleParameter", postToUpsert.PostTitle, DbType.String);
            sqlParameters.Add("@PostContentParameter", postToUpsert.PostContent, DbType.String);
            
            if (postToUpsert.PostId > 0)
            {
                sql += ", @PostId = @PostIdParameter";
                sqlParameters.Add("@PostIdParameter", postToUpsert.PostId, DbType.Int32);
            }

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to upsert post");
        }

        [HttpDelete("DeletePost/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Delete
                @PostId = @PostIdParameter,
                @UserId = @UserIdParameter";
            
            DynamicParameters sqlParameters = new DynamicParameters();
            
            sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post");
        }
    }
}