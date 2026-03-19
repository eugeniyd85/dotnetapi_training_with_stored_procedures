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

        [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = "SELECT * FROM TutorialAppSchema.Posts";
            return _dapper.LoadData<Post>(sql);
        }

        [HttpGet("PostSingle/{postId}")]
        public Post GetPostSingle(int postId)
        {
            string sql = "SELECT * FROM TutorialAppSchema.Posts WHERE PostId = " + postId.ToString();
            return _dapper.LoadDataSingle<Post>(sql);
        }

        [HttpGet("PostsByUser/{userId}")]
        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string sql = "SELECT * FROM TutorialAppSchema.Posts WHERE UserId = " + userId.ToString();
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

        [HttpGet("PostsBySearch/{searchParam}")]
        public IEnumerable<Post> GetPostsBySearch(string searchParam)
        {
            // this would be more complicated using Entity Framework and LINQ
            // maybe remove userId check?
            string sql = "SELECT * FROM TutorialAppSchema.Posts WHERE UserId = " + this.User.FindFirst("userId")?.Value + " AND (PostTitle LIKE '%" + searchParam + "%' OR PostContent LIKE '%" + searchParam + "%')";
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