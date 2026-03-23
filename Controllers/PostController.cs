using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    // [Authorize]
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
        public IEnumerable<Post> GetPosts(int postId, int userId, string searchParam = "None")
        {
            string sql = "EXEC TutorialAppSchema.spPosts_Get";
            string parameters = "";

            if (postId != 0)
            {
                parameters += ", @PostId = " + postId.ToString();
            }

            if (userId != 0)
            {
                parameters += ", @UserId = " + userId.ToString();
            }

            if (searchParam != "None")
            {
                parameters += ", @SearchValue = '" + searchParam + "'";
            }

            // compare with '!string.IsNullOrEmpty(parameters)' in UserController.cs, what is better to use or the same? => both are fine, but the one with 'parameters.Length > 0' is more efficient because it does not need to call a method, it just checks the length of the string, while the one with '!string.IsNullOrEmpty(parameters)' needs to call the IsNullOrEmpty method which checks if the string is null or empty, and then returns a boolean value, so it is less efficient than just checking the length of the string
            if (parameters.Length > 0)
            {
                sql += parameters.Substring(1); // remove the first comma
            }

            Console.WriteLine(sql);
            
            return _dapper.LoadData<Post>(sql);
        }

        // should only return posts of the user that is currently logged in, so we can get the user id from the token and use it in the sql query
        // how the user id from the token is getting? we can get it from the User property of the controller, which is of type ClaimsPrincipal, and we can use the FindFirst method to find the claim with the type "UserId" and get its value
        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = "SELECT * FROM TutorialAppSchema.Posts WHERE UserId = " + this.User.FindFirst("userId")?.Value; // this in this.User reffers to PostController class, and User is a property of the ControllerBase class that PostController inherits from, and it is of type ClaimsPrincipal which represents the current user and their claims, and we can use the FindFirst method to find the claim with the type "UserId" and get its value, and we can use it in the sql query to get only the posts of the currently logged in user
            return _dapper.LoadData<Post>(sql);
        }

        [HttpPost("AddPost")]
        public IActionResult AddPost(PostToAddDto postToAddDto)
        {
            // find out why here 'this.User.FindFirst("userId")?.Value' UserId caused and error but "userId" works fine
            string sql = "INSERT INTO TutorialAppSchema.Posts (UserId, PostTitle, PostContent, PostCreated, PostUpdated) VALUES (" + this.User.FindFirst("userId")?.Value + ", '" + postToAddDto.PostTitle + "', '" + postToAddDto.PostContent + "', GETDATE(), GETDATE())";
            
            Console.WriteLine(sql);

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to add post");
        }

        [HttpPut("EditPost")]
        public IActionResult EditPost(PostToEditDto postToEditDto)
        {
            string sql = "UPDATE TutorialAppSchema.Posts SET PostTitle = '" + postToEditDto.PostTitle + "', PostContent = '" + postToEditDto.PostContent + "', PostUpdated = GETDATE() WHERE PostId = " + postToEditDto.PostId.ToString() + " AND UserId = " + this.User.FindFirst("userId")?.Value;
            
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to edit post");
        }

        [HttpDelete("DeletePost/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = "DELETE FROM TutorialAppSchema.Posts WHERE PostId = " + postId.ToString() + " AND UserId = " + this.User.FindFirst("userId")?.Value;
            
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post");
        }
    }
}