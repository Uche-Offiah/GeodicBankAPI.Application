using Azure;
using GeodicBankAPI.Domain.Entities;
using GeodicBankAPI.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System;

namespace GeodicBankAPI.Controllers
{
  

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly FinancialDbContext _dbContext;
        public TransactionsController(IConfiguration config, FinancialDbContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] UserDto user)
        {
            //var authenticatedUser = AuthenticateUser(user.Username, user.Password);

            //if (authenticatedUser != null)
            //{
            //    var tokenString = GenerateJwtToken(authenticatedUser);
            //    return Ok(new { Token = tokenString });
            //}

            return Unauthorized();
        }

        [HttpPost("send")]
        public IActionResult SendMoney([FromBody] Transaction transaction)
        {
            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var sender = _dbContext.Users.FirstOrDefault(u => u.Id == senderId);
            if (sender == null)
                return NotFound("Sender not found");

            var receiver = _dbContext.Users.FirstOrDefault(u => u.Id == transaction.ReceiverUserId);
            if (receiver == null)
                return NotFound("Receiver not found");

            // Add custom logic to validate sender's balance, transaction amount, etc.
            // Example: Deduct money from sender, add money to receiver, and save transaction to database

            return Ok("Money sent successfully.");
        }

        [HttpPost("receive")]
        public IActionResult ReceiveMoney([FromBody] Transaction transaction)
        {
            var receiverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var receiver = _dbContext.Users.FirstOrDefault(u => u.Id == receiverId);
            if (receiver == null)
                return NotFound("Receiver not found");

            var sender = _dbContext.Users.FirstOrDefault(u => u.Id == transaction.SenderUserId);
            if (sender == null)
                return NotFound("Sender not found");

            // Add custom logic to validate transaction, update receiver's balance, and save transaction to database

            return Ok("Money received successfully.");
        }

    }
}
