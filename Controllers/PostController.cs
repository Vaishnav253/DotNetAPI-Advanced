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
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";
            string stringParameters = "";

            DynamicParameters sqlParameters = new DynamicParameters();
            if(postId != 0)
            {
                // stringParameters += ", @PostId =" + postId.ToString();
                stringParameters += ", @PostId=@PostIdParameter";
                sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);
            }
            if(userId != 0)
            {
                // stringParameters += ", @UserId =" + userId.ToString();
                stringParameters += ", @UserId=@UserIdParameter";
                sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            }
            if(searchParam.ToLower() != "none")
            {
                // stringParameters += ", @SearchValue ='" + searchParam + "'";
                stringParameters += ", @SearchValue=@SearchValueParameter";
                sqlParameters.Add("@SearchValueParameter", searchParam, DbType.String);
            }

            if(stringParameters.Length > 0)
            {
                sql += stringParameters.Substring(1);
            }

            return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
        }

        // [HttpGet("PostSingle/{postId}")]
        // // public IEnumerable<Post> GetPostSingle(int postId)
        // public Post GetPostSingle(int postId)
        // {
        //     string sql = @"SELECT [PostId],
        //             [UserId],
        //             [PostTitle],
        //             [PostContent],
        //             [PostCreated],
        //             [PostUpdated] 
        //         FROM TutorialAppSchema.Posts
        //             WHERE PostId = " + postId.ToString();

        //     return _dapper.LoadDataSingle<Post>(sql);
        // }

        // [HttpGet("PostsByUser/{userId}")]
        // public IEnumerable<Post> GetPostsByUser(int userId)
        // {
        //     string sql = @"SELECT [PostId],
        //             [UserId],
        //             [PostTitle],
        //             [PostContent],
        //             [PostCreated],
        //             [PostUpdated] 
        //         FROM TutorialAppSchema.Posts
        //             WHERE UserId = " + userId.ToString();

        //     return _dapper.LoadData<Post>(sql);
        // }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId=@UserIdParameter"; 
            // + this.User.FindFirst("userId")?.Value;
            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);

            return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
        }

        // [HttpGet("PostsBySearch/{searchParam}")]
        // public IEnumerable<Post> PostsBySearch(string searchParam)
        // {
        //     string sql = @"SELECT [PostId],
        //             [UserId],
        //             [PostTitle],
        //             [PostContent],
        //             [PostCreated],
        //             [PostUpdated] 
        //         FROM TutorialAppSchema.Posts
        //             WHERE PostTitle LIKE '%" + searchParam + "%'" +
        //                 " OR PostContent LIKE '%" + searchParam + "%'";

        //     return _dapper.LoadData<Post>(sql);
        // }

        static string EscapeSingleQuote(string input)
        {
            string output = input.Replace("'", "''");
 
            return output;
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            // string sql = @"TutorialAppSchema.spPosts_Upsert
            //     @UserId =" + this.User.FindFirst("userId")?.Value +
            //     ", @PostTitle ='" + EscapeSingleQuote(postToUpsert.PostTitle) +
            //     "', @PostContent ='" + EscapeSingleQuote(postToUpsert.PostContent) + "'";
            string sql = @"TutorialAppSchema.spPosts_Upsert
                @UserId = @UserIdParameter, 
                @PostTitle = @PostTitleParameter, 
                @PostContent = @PostContentParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParameters.Add("@PostTitleParameter", postToUpsert.PostTitle, DbType.String);
            sqlParameters.Add("@PostContentParameter", postToUpsert.PostContent, DbType.String);


            if(postToUpsert.PostId > 0)
            {
                // sql += ", @PostId = " + postToUpsert.PostId;
                sql += ", @PostId=@PostIdParameter";
                sqlParameters.Add("@PostIdParameter", postToUpsert.PostId, DbType.Int32);
            }
                
            if(_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Faled to Upsert posts!");    
        }
        
        // // [AllowAnonymous]
        // [HttpPut("Post")] 
        // public IActionResult EditPost(PostToEditDto postToEdit)
        // {
        //     string sql = @"
        //     UPDATE TutorialAppSchema.Posts 
        //         SET PostContent = '" + EscapeSingleQuote(postToEdit.PostContent) + 
        //         "', PostTitle = '" + EscapeSingleQuote(postToEdit.PostTitle) + 
        //         @"', PostUpdated = GETDATE()
        //         WHERE PostId = " + postToEdit.PostId.ToString() +
        //         "AND UserId = " + this.User.FindFirst("userId")?.Value;

        //     if(_dapper.ExecuteSql(sql))
        //     {
        //         return Ok();
        //     }

        //     throw new Exception("Faled to edit post!");    
        // }

        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            // string sql = @"EXEC TutorialAppSchema.spPost_Delete @PostId = " + 
            //         postId.ToString() +
            //         ", @UserId = " + this.User.FindFirst("userId")?.Value;
            string sql = @"EXEC TutorialAppSchema.spPost_Delete
                @UserId=@UserIdParameter, 
                @PostId=@PostIdParameter";
            
            Console.WriteLine("Generated SQL Query for Deletion: " + sql);

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParameters.Add("@PostIdParameter", postId, DbType.Int32);
            
            if(_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Faled to delete posts!");
        }
    }
}