using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using DotnetAPI.Helpers;
using Dapper;
using AutoMapper;
using DotnetAPI.Models;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        // private readonly IConfiguration _config;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            // _config = config;
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(new MapperConfiguration(cfg => 
            {
                cfg.CreateMap<UserForRegistrationDto, UserComplete>();
            }));
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if(userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + 
                    userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if(existingUsers.Count() == 0)
                {
                    // byte[] passwordSalt = new byte[128 / 8];
                    // using(RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    // {
                    //     rng.GetNonZeroBytes(passwordSalt);
                    // }

                    // byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    // string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert 
                    //     @Email = @EmailParam, 
                    //     @PasswordHash = @PasswordHashParam, 
                    //     @PasswordSalt = @PasswordSaltParam";

                    //     List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    //     SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
                    //     emailParameter.Value = userForRegistration.Email;
                    //     sqlParameters.Add(emailParameter);

                    //     SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParam", SqlDbType.VarBinary);
                    //     passwordHashParameter.Value = passwordHash;
                    //     sqlParameters.Add(passwordHashParameter);

                    //     SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSaltParam", SqlDbType.VarBinary);
                    //     passwordSaltParameter.Value = passwordSalt;
                    //     sqlParameters.Add(passwordSaltParameter);

                        UserForLoginDto userForSetPassword = new UserForLoginDto() {
                            Email = userForRegistration.Email,
                            Password = userForRegistration.Password
                        };
                        if(_authHelper.SetPassword(userForSetPassword))
                        {
                            UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);
                            userComplete.Active = true;

                            // string sqlAddUser = @"EXEC TutorialAppSchema.spUser_Upsert
                            // @FirstName = '" + userForRegistration.FirstName + 
                            // "', @LastName = '" + userForRegistration.LastName +
                            // "', @Email = '" + userForRegistration.Email + 
                            // "', @Gender = '" + userForRegistration.Gender + 
                            // "', @Active = 1" + 
                            // ", @JobTitle = '" + userForRegistration.JobTitle + 
                            // "', @Department = '" + userForRegistration.Department + 
                            // "', @Salary = '" + userForRegistration.Salary + "'";
                            // // string sqlAddUser = @"INSERT INTO TutorialAppSchema.Users(
                            // //         [FirstName],
                            // //         [LastName],
                            // //         [Email],
                            // //         [Gender],
                            // //         [Active]
                            // //     ) VALUES (" +
                            // //         "'" + userForRegistration.FirstName +
                            // //         "', '" + userForRegistration.LastName +
                            // //         "', '" + userForRegistration.Email +
                            // //         "', '" + userForRegistration.Gender +
                            // //         "', 1)";

                            if(_reusableSql.UpsertUser(userComplete))
                            {
                                return Ok();
                            }
                            throw new Exception("Failed to add the user.");
                        }
                        throw new Exception("Failed to register the user.");
                }
                throw new Exception("User with this Email already exists!");
            }
            throw new Exception("Password do not match!");
        }

        [HttpPut("ResetPaswword")]
        public IActionResult ResetPaswword(UserForLoginDto userForSetPassword)
        {
            if(_authHelper.SetPassword(userForSetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to update Password!");
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

            // if (passwordHash == userForConfirmation.PasswordHash) // Won't work

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index]){
                    return StatusCode(401, "Incorrect password!");
                }
            }

            string userIdSql = @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" +
                userForLogin.Email + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string>{
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        // public IActionResult RefreshToken()
        public string RefreshToken()
        {
            // string userId = User.FindFirst("userId")?.Value + "";

            // string userIdSql = "SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = "
            //     +userId;

            string userIdSql = @"
                SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = '" + 
                    User.FindFirst("userId")?.Value + "'";

            // int userIdFromDB = _dapper.LoadDataSingle<int>(userIdSql);

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return _authHelper.CreateToken(userId);

            // return Ok(new Dictionary<string, string>{
            //     {"token", CreateToken(userIdFromDB)}
            // });
        }

        
    }
}
