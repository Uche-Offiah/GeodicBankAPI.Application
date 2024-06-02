using Azure;
using GeodicBankAPI.Application.Interfaces;
using GeodicBankAPI.Domain;
using GeodicBankAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GeodicBankAPI.Application.Services
{
    public class UserService : IUser
    {
        private readonly FinancialDbContext _context;
        private readonly IConfiguration _configuration;
        public UserService(IConfiguration configuration,FinancialDbContext context)
        {
            _context = context;
            _configuration = configuration;
        }
        public User GetUser(string Id)
        {
            var response = new User();

            return response;
        }

        public User UserDetails(string Id)
        {
            var response = new User();

            return response;
        }

        public async Task<string> CreateJwtToken(User user)
        {
            var getUser = await _context.Users.SingleOrDefaultAsync(x => x.UserName == user.UserName);
            if (getUser != null)
            {
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var signingCredential = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);

                var subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                });
                var expires = DateTime.Now.AddMinutes(10);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Expires = expires,
                    Issuer = issuer,
                    Audience = audience,
                    Subject = subject,
                    SigningCredentials = signingCredential
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);

                return jwtToken;
            }
            return string.Empty;
        }

        public async Task<bool> UsernameExist(string username)
        {

            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        private async Task<User> AuthenticateWithHMACSHA512(string Username, string Password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == Username);
            if (user == null)
            {
                return new User { };
            }
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(Password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.Password[i])
                {
                    return new User { };
                }
            }

            return new User
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,  
            };
        }

        public async Task<object> RegisterUser(User userDto)
        {
            using var hmac = new HMACSHA512();
            var user = new  User
            {
                UserName = userDto.UserName,
                Password = hmac.ComputeHash(userDto.Password),
                PasswordSalt = hmac.Key,
                City = userDto.City,
                Country = userDto.Country,
                DateOfBirth = userDto.DateOfBirth
            };

            _context.Add(user);
            await _context.SaveChangesAsync();

            return new
            {
                UserName = user.UserName,
                Token = CreateJwtToken(user),
            };
        }

        public async Task<bool> AuthenticateUser(string username, string password)
        {
           User user = await AuthenticateWithHMACSHA512(username, password);
            if (user != null)
            {
                return true;
            }
            return false;
        }
    }
}
