using GeodicBankAPI.Domain.Entities;
using GeodicBankAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeodicBankAPI.Application.Interfaces;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GeodicBankAPI.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly FinancialDbContext _dbContext;
        private readonly IUser _user;
        public UserController(IConfiguration config, FinancialDbContext dbContext, IUser user)
        {
            _config = config;
            _dbContext = dbContext;
            _user = user;
        }
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] UserDto user)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authenticatedUser = _user.AuthenticateUser(user.Username, user.Password);

            //if (authenticatedUser != null)
            //{
            //    var tokenString = GenerateJwtToken(authenticatedUser);
            //    return Ok(new { Token = tokenString });
            //}
            return Unauthorized();
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register([FromBody]UserDto userDto)
        {

            if (await _user.UsernameExist(userDto.Username)) return BadRequest("UserName already Exists");

            var user = new User
            { 
                UserName = userDto.Username,
                Password =  Encoding.UTF8.GetBytes(userDto.Password),
            };

            var result = await _user.RegisterUser(user);
            if (result != null)
            {
                return Ok(result);
            }
            
            return BadRequest(result);

        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(User login)
        {

           return Ok();
        }

        
    }
}
