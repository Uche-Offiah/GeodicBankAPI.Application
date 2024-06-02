using GeodicBankAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeodicBankAPI.Application.Interfaces
{
    public interface IUser
    {
        public User GetUser(string Id);
        public User UserDetails(string Id);
        public Task<bool> AuthenticateUser(string Username, string Password);
        public Task<string> CreateJwtToken(User user);
        public Task<bool> UsernameExist(string Username);
        public Task<object> RegisterUser(User user);
    }
}
