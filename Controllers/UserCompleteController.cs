using Microsoft.AspNetCore.Mvc;
using DotnetAPI.Data;
using DotnetAPI.Models;
using DotnetAPI.Dtos;
using Dapper;
using System.Data;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace DotnetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserCompleteController: ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;
    public UserCompleteController(IConfiguration config)
    {
        // Console.WriteLine(config.GetConnectionString("DefaultConnection"));
        _dapper = new DataContextDapper(config);
        _reusableSql = new ReusableSql(config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet("GetUsers/{userId}/{isActive}")]
    // public IEnumerable<User> GetUsers()
    // public IActionResult Test()

     public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get"; 
        string stringParameters = "";
        DynamicParameters sqlParameters = new DynamicParameters();
        
        if(userId != 0)
        {
            // parameters += ", @UserId =" + userId.ToString();
            stringParameters += ", @UserId=@UserIdParameter";
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        }
        if(isActive)
        {
            // parameters += ", @Active = " + isActive.ToString();
            stringParameters += ", @Active=@ActiveParameter";
            sqlParameters.Add("@ActiveParameter", isActive, DbType.Boolean);
        }

        if(stringParameters.Length > 0)
        {
            sql += stringParameters.Substring(1);
        }

        // Console.WriteLine(sql);

        // IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
        IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql, sqlParameters);
        return users;
        // // string[] responseArray = new string[] 
        // // return new string[] {
        // //     "test1",
        // //     "test2"
        // // };

        // // return responseArray;

        // // return Enumerable.Range(1, 5).Select(index = new WeatherForecast{
        // //     Date = DateTime.Now.AddDays(index),
        // //     TemperatureC = Random.Shared.Next(-20, 55),
        // //     Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        // // })
        // // .ToArray();
    }

    // [HttpGet("GetSingleUser/{userId}")]
    // // public IEnumerable<User> GetUsers()
    // // public IActionResult Test()

    // public User GetSingleUser(int userId)
    // {
    //     string sql = @"
    //         SELECT [UserId],
    //             [FirstName],
    //             [LastName],
    //             [Email],
    //             [Gender],
    //             [Active] 
    //         FROM TutorialAppSchema.Users
    //             WHERE UserId = " + userId.ToString();
    //     User user = _dapper.LoadDataSingle<User>(sql);
    //     return user;

    //     // string[] responseArray = new string[] 
    //     // return new string[] {
    //     //     "test1",
    //     //     "test2",
    //     //     testValue
    //     // };

    //     // return responseArray;

    //     // return Enumerable.Range(1, 5).Select(index = new WeatherForecast{
    //     //     Date = DateTime.Now.AddDays(index),
    //     //     TemperatureC = Random.Shared.Next(-20, 55),
    //     //     Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //     // })
    //     // .ToArray();
    // }

    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {

            if(_reusableSql.UpsertUser(user))
            {
                return Ok();
            }

            throw new Exception("Faled to Update the User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        // string sql = @"TutorialAppSchema.spUser_Delete 
        //         @UserId = " + userId.ToString();

        string sql = @"TutorialAppSchema.spUser_Delete 
                @UserId = @UserIdParameter";

                // Console.WriteLine(sql);

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);

            if(_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Faled to Delete the User");
    }
}