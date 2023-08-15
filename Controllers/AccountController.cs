using System.Security.Cryptography;
using System.Text;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.DTOs;
using API.Interfaces;
using System.Data.SqlClient;
using System.Data;
using Dapper;
namespace API.Controllers
{
    [ApiController]
[Route("[controller]")]
        public class AccountController : ControllerBase
        {
                private readonly ITokenService _tokenService;
                // private readonly DataContext _context;
                string connectionString = "Server=ANAND-BABU;Database=bikestores;Trusted_Connection=True";

             
                public AccountController(ITokenService tokenService)
                {
                        _tokenService = tokenService;
                      
                        // _context = context;
                    
                }
                [HttpPost("Register")] //post:/api/account/register
                public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
                {
                   using var hmac=new HMACSHA512();//used to create password salt
                   var user=new UserData
                   {
                         LoginName=registerDto.Username.ToLower(),
                         PasswordHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                         PasswordSalt=hmac.Key
                   };
                   using IDbConnection dbConnection = new SqlConnection(connectionString);
                   dbConnection.Open();
                //    string query=" INSERT INTO dbo.[User] (LoginName) VALUES('arun')";
                   string query=" INSERT INTO dbo.[User] (LoginName, PasswordHash, PasswordSalt) VALUES(@name,@pass1,@pass2)";
                var parameters = new DynamicParameters();
                parameters.Add("name", user.LoginName);
                parameters.Add("pass1", user.PasswordHash);
                parameters.Add("pass2", user.PasswordSalt);
                   var result=dbConnection.Execute(query,parameters);
                  
                   return new UserDto{
                        Username=user.LoginName,
                        Token=_tokenService.CreateToken(user)
                   };
                }

                [HttpPost("Login")]
               public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
               {
                string connectionString = "Server=ANAND-BABU;Database=bikestores;Trusted_Connection=True";
                using IDbConnection dbConnection = new SqlConnection(connectionString);
                string query = "SELECT * FROM dbo.[User] WHERE LoginName = @loginname";
                string loginname = loginDto.Username;
                var user = dbConnection.Query<UserData>(query, new { LoginName = loginname });
                      if(user==null) return Unauthorized("Invalid username");
                      if(user.Count()==0){
                        return Unauthorized("invalid user.");
                      }

                       using var hmac=new HMACSHA512(user.ToList()[0].PasswordSalt);
                       var computeHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
                       for(int i=0;i<computeHash.Length;i++)
                       {
                           if(computeHash[i]!=user.ToList()[0].PasswordHash[i]) return Unauthorized("Invalid password");
                       }
                       return  new UserDto
                       {
                        Username=user.ToList()[0].LoginName,
                        Token=_tokenService.CreateToken(user.ToList()[0])
                   };

               }
        }
}
